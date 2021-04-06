using HioldMod;
using HioldMod.src.UserTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConfigTools
{
    public class LoadSignCardMainConfig
    {

        public const string version = "19.3.2";
        public static string Server_Response_Name = "[FFCC00]HioldSignCardMod";
        public static string Chat_Response_Color = "[00FF00]";
        private const string configFile = "ActionKillAwardConfig.xml";
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
            Log.Out("[HioldSignCardMod] 签到月卡MOD----验证配置文件 & 保存新Entity");
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
                //是否开启
                if (childNode.Name == "Enable")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] Enable， 缺失'Enable' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!bool.TryParse(_line.GetAttribute("value"), out KillConfig.IsEnable))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] Enable配置有误"));
                        }
                    }
                }

                //默认僵尸击杀配置
                if (childNode.Name == "DefaultZombie")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] DefaultZombie， 缺失'DefaultZombie' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!int.TryParse(_line.GetAttribute("value"), out KillConfig.defaultZombie))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] DefaultZombie配置有误"));
                        }
                    }
                }

                //默认动物击杀配置
                if (childNode.Name == "DefaultAnimal")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] DefaultAnimal， 缺失'DefaultAnimal' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!int.TryParse(_line.GetAttribute("value"), out KillConfig.defaultAnimal))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] DefaultAnimal配置有误"));
                        }
                    }
                }

                //是否开启僵尸击杀奖励
                if (childNode.Name == "ZombieEnable")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] ZombieEnable， 缺失'ZombieEnable' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!bool.TryParse(_line.GetAttribute("value"), out KillConfig.IsZombleEnable))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] ZombieEnable配置有误"));
                        }
                    }
                }

                //是否开启动物击杀奖励
                if (childNode.Name == "AnimalEnable")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] AnimalEnable， 缺失'AnimalEnable' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!bool.TryParse(_line.GetAttribute("value"), out KillConfig.IsAnimalEnable))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] AnimalEnable配置有误"));
                        }
                    }
                }


                //是否开启血月双倍
                if (childNode.Name == "BloodMoonDoubleEnable")
                {
                    XmlElement _line = (XmlElement)childNode;
                    if (!_line.HasAttribute("value"))
                    {
                        Log.Warning(string.Format("[HioldSignCardMod] BloodMoonDoubleEnable， 缺失'BloodMoonDoubleEnable' 属性: {0}", _line.OuterXml));
                        continue;
                    }
                    else
                    {
                        if (!bool.TryParse(_line.GetAttribute("value"), out KillConfig.IsBloodMoonDoubleEnable))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] BloodMoonDoubleEnable配置有误"));
                        }
                    }
                }


                //僵尸击杀奖励列表
                if (childNode.Name == "ZombleKillAward")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] 在HioldSignCardMod存在不支持的 section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        //奖励量
                        if (!_line.HasAttribute("enable"))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] value， 缺失'enable' 属性: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            if (_line.HasAttribute("id") && _line.HasAttribute("value") && _line.HasAttribute("enable"))
                            {
                                //检测到为有效任务配置
                                if (_line.GetAttribute("enable").Equals("True"))
                                {
                                    if (!int.TryParse(_line.GetAttribute("value"), out int tmp))
                                    {
                                        KillConfig.zombieAward.Add(_line.GetAttribute("id"), tmp);
                                    }
                                    else
                                    {
                                        Log.Warning(string.Format("[HioldSignCardMod] 击杀奖励配置有误,value值异常"));
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning(string.Format("[HioldSignCardMod] 击杀奖励配置有误,缺少属性"));
                            }
                        }
                    }
                }



                //动物击杀奖励列表
                if (childNode.Name == "AnimalKillAward")
                {
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] 在HioldSignCardMod存在不支持的 section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        //奖励量
                        if (!_line.HasAttribute("enable"))
                        {
                            Log.Warning(string.Format("[HioldSignCardMod] value， 缺失'enable' 属性: {0}", subChild.OuterXml));
                            continue;
                        }
                        else
                        {
                            if (_line.HasAttribute("id") && _line.HasAttribute("value") && _line.HasAttribute("enable"))
                            {
                                //检测到为有效任务配置
                                if (_line.GetAttribute("enable").Equals("True"))
                                {
                                    if (!int.TryParse(_line.GetAttribute("value"), out int tmp))
                                    {
                                        KillConfig.animalAward.Add(_line.GetAttribute("id"), tmp);
                                    }
                                    else
                                    {
                                        Log.Warning(string.Format("[HioldSignCardMod] 击杀奖励配置有误,value值异常"));
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning(string.Format("[HioldSignCardMod] 击杀奖励配置有误,缺少属性"));
                            }
                        }
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
                //是否开启
                sw.WriteLine(string.Format("    <Enable value=\"{0}\" />", "False"));
                sw.WriteLine("    <!--是否开启击杀奖励 True/False-->");
                //僵尸默认奖励值
                sw.WriteLine(string.Format("    <DefaultZombie value=\"{0}\" />", "0"));
                sw.WriteLine("    <!--默认击杀奖励积分(未配置奖励的击杀,发放这个配置的数量)-->");
                //动物默认奖励值
                sw.WriteLine(string.Format("    <DefaultAnimal value=\"{0}\" />", "0"));
                sw.WriteLine("    <!--默认击杀奖励积分(未配置奖励的击杀,发放这个配置的数量)-->");
                //是否开启僵尸击杀奖励
                sw.WriteLine(string.Format("    <ZombieEnable value=\"{0}\" />", "False"));
                sw.WriteLine("    <!--是否开启僵尸击杀奖励-->");
                //是否开启动物击杀奖励
                sw.WriteLine(string.Format("    <AnimalEnable value=\"{0}\" />", "False"));
                sw.WriteLine("    <!--是否开启动物击杀奖励-->");
                //是否开启血月双倍奖励
                sw.WriteLine(string.Format("    <BloodMoonDoubleEnable value=\"{0}\" />", "False"));
                sw.WriteLine("    <!--是否开血月双倍模式-->");

                //僵尸击杀
                sw.WriteLine("    <ZombleKillAward>");
                sw.WriteLine(string.Format("        <ZombieKill Name=\"{0}\" id=\"{1}\" value=\"{2}\" enable=\"{3}\" />", "测试僵尸类型", "testzomble", "1", "False"));
                sw.WriteLine("        <!--Name属性非必须,id为僵尸类型，value为发放数量,enable为是否开启(若设置成False则无效,使用Default设置的默认值进行发放)-->");
                sw.WriteLine("    </ZombleKillAward>");



                //动物击杀
                sw.WriteLine("    <AnimalKillAward>");
                sw.WriteLine(string.Format("        <AnimalKill Name=\"{0}\" id=\"{1}\" value=\"{2}\" enable=\"{3}\" />", "测试动物类型", "testAnimal", "1", "False"));
                sw.WriteLine("        <!--Name属性非必须,id为动物类型，value为发放数量,enable为是否开启(若设置成False则无效,使用Default设置的默认值进行发放)-->");
                sw.WriteLine("    </AnimalKillAward>");

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
                            _oldXml.Load(OldXmlDirectory + "/ActionMainConfig.xml");
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