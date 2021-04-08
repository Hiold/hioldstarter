using HioldMod;
using HioldMod.src.Serialize;
using HioldMod.src.UserTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConfigTools
{
    public class LoadSignCardInfoConfig
    {

        public const string version = "19.3.2";
        public static string Server_Response_Name = "[FFCC00]HioldSignCardMod";
        public static string Chat_Response_Color = "[00FF00]";
        private const string configFile = "SignCardInfoConfig.xml";
        public static string configFilePath = string.Format("{0}/{1}", API.ConfigPath, configFile);
        public static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, configFile);
        public static string OldXmlDirectory = "";

        public static void Load()
        {
            //Log.Out("[HioldSignCardMod] 文件路径" + configFilePath);
            LoadXml();
            InitFileWatcher();
        }

        public static void LoadXml()
        {
            //清空原有数据
            SignCardConfig.awrodCardList.Clear();
            //
            Log.Out("---------------------------------------------------------------");
            Log.Out("[HioldSignCardMod] 签到月卡MOD奖励配置----验证配置文件 & 保存新Entity");
            Log.Out("---------------------------------------------------------------");
            if (!Utils.FileExists(configFilePath))
            {
                WriteXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(configFilePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[HioldSignCardMod] 加载错误 {0}: {1}", configFilePath, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                //解析配置数据
                if (childNode.Name == "Group")
                {
                    XmlElement _inNode = (XmlElement)childNode;

                    if (!_inNode.HasAttribute("name"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] Group标签缺少name属性"));
                        continue;
                    }

                    string groupName = _inNode.GetAttribute("name");
                    //奖励列表
                    List<List<CardAwordInfo>> AwordList = new List<List<CardAwordInfo>>();
                    //初始化奖励列表数据
                    //List<CardAwordInfo> cardAwordInfos = new List<CardAwordInfo>();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        //初始化集合
                        if (AwordList.Count == 0)
                        {
                            AwordList.Add(new List<CardAwordInfo>());
                        }

                        XmlElement _line = (XmlElement)subChild;
                        //判断类型
                        if (_line.Name.Equals("Award"))
                        {
                            //每日重复奖励
                            if (subChild.NodeType == XmlNodeType.Comment)
                            {
                                continue;
                            }
                            if (subChild.NodeType != XmlNodeType.Element)
                            {
                                Log.Warning(string.Format("[HioldSignCardMod] 在HioldSignCardMod存在不支持的 section: {0}", subChild.OuterXml));
                                continue;
                            }

                            if (_line.HasAttribute("type") && _line.HasAttribute("enable"))
                            {
                                //检测到为有效任务配置
                                if (_line.GetAttribute("enable").Equals("True"))
                                {
                                    //判断奖励类型
                                    if (_line.GetAttribute("type").Equals("item"))
                                    {
                                        //string awardtype, string command, string itemname, int itemquality, int itemcount
                                        //转换数据类型
                                        if (int.TryParse(_line.GetAttribute("itemquality"), out int itemquality))
                                        {
                                            if (int.TryParse(_line.GetAttribute("itemcount"), out int itemcount))
                                            {
                                                //转换成功配置无异常
                                                //物品奖励添加到集合中
                                                //cardAwordInfos.Add();
                                                AwordList[0].Add(new CardAwordInfo(_line.GetAttribute("type"), null, _line.GetAttribute("itemname"), itemquality, itemcount));
                                            }
                                            else
                                            {
                                                Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性itemcount"));
                                                continue;
                                            }

                                        }
                                        else
                                        {
                                            Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性itemquality"));
                                            continue;
                                        }

                                    }
                                    else if (_line.GetAttribute("type").Equals("command"))
                                    {
                                        //物品奖励添加到集合中
                                        //cardAwordInfos.Add(new CardAwordInfo(_line.GetAttribute("type"), _line.GetAttribute("command"), null, 0, 0));
                                        AwordList[0].Add(new CardAwordInfo(_line.GetAttribute("type"), _line.GetAttribute("command"), null, 0, 0));
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性"));
                            }
                            //处理单个奖励完毕
                        }
                        else if (_line.Name.Equals("Day"))
                        {
                            //处理每日不同


                            //初始化List
                            List<CardAwordInfo> cardAwordInfos = new List<CardAwordInfo>();
                            foreach (XmlNode _lineDay in _line.ChildNodes)
                            {
                                XmlElement _dayline = (XmlElement)_lineDay;
                                //每日重复奖励
                                if (subChild.NodeType == XmlNodeType.Comment)
                                {
                                    continue;
                                }
                                if (subChild.NodeType != XmlNodeType.Element)
                                {
                                    Log.Warning(string.Format("[HioldSignCardMod] 在HioldSignCardMod存在不支持的 section: {0}", subChild.OuterXml));
                                    continue;
                                }

                                if (_dayline.HasAttribute("type") && _dayline.HasAttribute("enable"))
                                {
                                    //检测到为有效任务配置
                                    if (_dayline.GetAttribute("enable").Equals("True"))
                                    {
                                        //判断奖励类型
                                        if (_dayline.GetAttribute("type").Equals("item"))
                                        {
                                            //string awardtype, string command, string itemname, int itemquality, int itemcount
                                            //转换数据类型
                                            if (int.TryParse(_dayline.GetAttribute("itemquality"), out int itemquality))
                                            {
                                                if (int.TryParse(_dayline.GetAttribute("itemcount"), out int itemcount))
                                                {
                                                    //转换成功配置无异常
                                                    //物品奖励添加到集合中
                                                    cardAwordInfos.Add(new CardAwordInfo(_dayline.GetAttribute("type"), null, _dayline.GetAttribute("itemname"), itemquality, itemcount));
                                                    //AwordList.Add(cardAwordInfos);
                                                }
                                                else
                                                {
                                                    Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性itemcount"));
                                                    continue;
                                                }

                                            }
                                            else
                                            {
                                                Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性itemquality"));
                                                continue;
                                            }

                                        }
                                        else if (_dayline.GetAttribute("type").Equals("command"))
                                        {
                                            //物品奖励添加到集合中
                                            cardAwordInfos.Add(new CardAwordInfo(_dayline.GetAttribute("type"), _dayline.GetAttribute("command"), null, 0, 0));
                                            //AwordList.Add(cardAwordInfos);
                                        }
                                    }
                                }
                                else
                                {
                                    Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性"));
                                }
                                //处理单个奖励完毕
                            }
                            AwordList.Add(cardAwordInfos);







                        }
                    }
                    //处理完毕添加集合进nama映射中
                    SignCardConfig.awrodCardList[groupName] = AwordList;
                }
            }
        }

        public static void WriteXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(configFilePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<HioldSignCardModConfig>");

                //物品分组信息
                //默认
                sw.WriteLine("    <Group name=\"default\">");
                sw.WriteLine("        <Award name=\"木头\" type=\"item\" itemname=\"resourceWood\" itemquality=\"0\" itemcount=\"100\" enable=\"True\" />");
                sw.WriteLine("        <Award name=\"石头\" type=\"item\" itemname=\"resourceRockSmall\" itemquality=\"0\" itemcount=\"100\" enable=\"True\" />");
                sw.WriteLine("        <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取\" enable=\"True\" />");
                sw.WriteLine("        <!--Name属性非必须，Type为奖励类型(item:物品 command:执行命令。{username}填充用户名，{usersteamid}填充Steam ID)，itemname为物品名，itemquality为物品品质（无品质物品请写0），itemcount为物品数量，enable为是否可用（True可用正常发放，False停用不发放）-->");
                sw.WriteLine("    </Group>");

                //物品分组信息
                //VIP
                sw.WriteLine("    <Group name=\"vip\">");
                sw.WriteLine("        <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"100\" enable=\"True\" />");
                sw.WriteLine("        <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"100\" enable=\"True\" />");
                sw.WriteLine("        <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP礼包\" enable=\"True\" />");
                sw.WriteLine("    </Group>");
                sw.WriteLine("    <!--以上是签到默认奖励示例配置(每天领取到的物品一样)-->");
                sw.WriteLine("    <!--以下是周卡配置示例(为签到的每天配置不同物品)-->");

                //周卡配置
                sw.WriteLine("    <Group name=\"vipWeek\">");
                sw.WriteLine("        <Day value=\"1\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"1\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"1\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第一天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"2\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"2\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"2\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第二天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"3\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"3\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"3\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第三天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"4\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"4\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"4\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第四天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"5\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"5\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"5\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第五天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"6\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"6\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"6\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第六天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("        <Day value=\"7\">");
                sw.WriteLine("            <!--value值为对应天数,请注意不要重复,否则可能出现覆盖问题-->");
                sw.WriteLine("            <Award name=\"锻铁\" type=\"item\" itemname=\"resourceForgedIron\" itemquality=\"0\" itemcount=\"7\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"锻钢\" type=\"item\" itemname=\"resourceForgedSteel\" itemquality=\"0\" itemcount=\"7\" enable=\"True\" />");
                sw.WriteLine("            <Award name=\"命令\" type=\"command\" command=\"sayplayer {username} [00ff00]成功领取VIP周卡第七天奖励\" enable=\"True\" />");
                sw.WriteLine("        </Day");
                sw.WriteLine("    </Group>");


                //配置文件结束
                sw.WriteLine("</HioldSignCardModConfig>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            LoadXml();
        }

        public static void UpgradeXml()
        {
            try
            {
                if (OldXmlDirectory != "")
                {
                    if (Utils.FileExists(configFilePath))
                    {
                        XmlDocument _oldXml = new XmlDocument();
                        try
                        {
                            _oldXml.Load(OldXmlDirectory + "/SignCardInfoConfig.xml");
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[HioldSignCardMod] 加载错误 {0}: {1}", OldXmlDirectory + "/ActionMainConfig.xml", e.Message));
                            return;
                        }
                        XmlNode _oldXmlNode = _oldXml.DocumentElement;
                        XmlNodeList _oldNodeList = _oldXmlNode.ChildNodes;
                        XmlDocument _newXml = new XmlDocument();
                        try
                        {
                            _newXml.Load(configFilePath);
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[HioldSignCardMod] 加载错误 {0}: {1}", configFilePath, e.Message));
                            return;
                        }
                        XmlNode _newXmlNode = _newXml.DocumentElement;
                        XmlNodeList _newNodeList = _newXmlNode.ChildNodes;
                        for (int i = 0; i < _oldNodeList.Count; i++)
                        {
                            XmlNode _oldChildNode = _oldNodeList[i];
                            if (_oldChildNode.Name == "HioldSignCardModConfig")
                            {
                                for (int j = 0; j < _oldChildNode.ChildNodes.Count; j++)
                                {
                                    XmlNode _oldSubChild = _oldChildNode.ChildNodes[j];
                                    if (_oldSubChild.Name == "Option")
                                    {
                                        XmlElement _oldElement = (XmlElement)_oldSubChild;
                                        XmlAttributeCollection _attributes = _oldElement.Attributes;
                                        string _elementName = _attributes[0].Value;
                                        for (int k = 1; k < _attributes.Count; k++)
                                        {
                                            XmlAttribute _oldAttribute = _attributes[k];
                                            SetXml(_newXml, _newNodeList, _elementName, _oldAttribute);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[HioldSignCardMod] Error in LoadConfig.UpgradeXml: {0}", e.Message));
            }
        }

        private static void SetXml(XmlDocument _newXml, XmlNodeList _newNodeList, string _elementName, XmlAttribute _oldAttribute)
        {
            try
            {
                for (int i = 0; i < _newNodeList.Count; i++)
                {
                    XmlNode _newChildNode = _newNodeList[i];
                    if (_newChildNode.Name == "HioldSignCardMod")
                    {
                        for (int j = 0; j < _newChildNode.ChildNodes.Count; j++)
                        {
                            XmlNode _newSubChild = _newChildNode.ChildNodes[j];
                            if (_newSubChild.Name == "Option")
                            {
                                XmlElement _newElement = (XmlElement)_newSubChild;
                                XmlAttributeCollection _newAttributes = _newElement.Attributes;
                                if (_newElement.Attributes[0].Value == _elementName)
                                {
                                    for (int k = 1; k < _newElement.Attributes.Count; k++)
                                    {
                                        XmlAttribute _newAttribute = _newElement.Attributes[k];
                                        if (_newAttribute.Name == _oldAttribute.Name)
                                        {
                                            if (_newAttribute.Value != _oldAttribute.Value)
                                            {
                                                _newAttribute.Value = _oldAttribute.Value;
                                                _newXml.Save(configFilePath);
                                            }
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[HioldSignCardMod] Error in LoadConfig.SetXml: {0}", e.Message));
            }
        }
    }
}