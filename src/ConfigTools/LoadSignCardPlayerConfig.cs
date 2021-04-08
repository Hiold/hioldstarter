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
    public class LoadSignCardPlayerConfig
    {

        public const string version = "19.3.2";
        public static string Server_Response_Name = "[FFCC00]HioldSignCardMod";
        public static string Chat_Response_Color = "[00FF00]";
        private const string configFile = "SignCardPlayerConfig.xml";
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
            SignCardConfig.playerGroup.Clear();
            //
            Log.Out("---------------------------------------------------------------");
            Log.Out("[HioldSignCardMod] 签到月卡MOD玩家分组配置----验证配置文件 & 保存新Entity");
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
                if (childNode.Name == "Player")
                {
                    XmlElement _inNode = (XmlElement)childNode;
                    if (!_inNode.HasAttribute("steamid"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] Group标签缺少steamid属性"));
                        continue;
                    }

                    if (_inNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    if (_inNode.NodeType != XmlNodeType.Element)
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] 在HioldSignCardMod存在不支持的 section: {0}", _inNode.OuterXml));
                        continue;
                    }

                    if (_inNode.HasAttribute("steamid") && _inNode.HasAttribute("enable") && _inNode.HasAttribute("group"))
                    {
                        //检测到为有效任务配置
                        if (_inNode.GetAttribute("enable").Equals("True"))
                        {
                            //保存玩家分组信息
                            //处理完毕添加集合进nama映射中
                            SignCardConfig.playerGroup[_inNode.GetAttribute("steamid")] = _inNode.GetAttribute("group");
                        }
                    }
                    else
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] 签到月卡配置有误,缺少属性"));
                    }


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

                sw.WriteLine("    <Player name=\"海鸥Design\" steamid=\"27099982312311221123\" group=\"default\" type=\"single\" enable=\"True\" />");
                sw.WriteLine("    <Player name=\"海鸥Program\" steamid=\"27099982312311222222\" group=\"vip\" type=\"single\" enable=\"True\" />");
                sw.WriteLine("    <Player name=\"海鸥Release\" steamid=\"27099982312311222222\" group=\"vipWeek\" type=\"multi\" enable=\"True\" startdate=\"20210408\" />");
                sw.WriteLine("    <!--Name属性非必须用于区分用户，steamid为玩家steamid，group为发放奖励列表（对应SignCardInfoConfig.xml里Group节点的name属性），enable为是否启用该用户配置（True/False）,若为False调用default-->");
                sw.WriteLine("    <!--type为single时发放签到奖励(每天一样的物品)。 type为multi时发放每天单独配置奖励，type为multi时必须配置生效日期(从哪天开始发放奖励)-->");

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
                            _oldXml.Load(OldXmlDirectory + "/SignCardPlayerConfig.xml");
                        }
                        catch (XmlException e)
                        {
                            Log.Error(string.Format("[HioldSignCardMod] 加载错误 {0}: {1}", OldXmlDirectory + "/SignCardPlayerConfig.xml", e.Message));
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