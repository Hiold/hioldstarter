using HioldMod.src.UserTools;
using ConfigTools;
using System.IO;
using System.Collections.Generic;
using ServerTools;
using System;

namespace HioldMod
{
    public class API : IModApi
    {
        //游戏路径
        public static string GamePath = Directory.GetCurrentDirectory();
        //config路径
        public static string ConfigPath = string.Format("{0}/Mods/HioldMod_funcs/config/", GamePath);

        public void InitMod()
        {

            //监听服务器初始化成功事件
            //ModEvents.GameStartDone.RegisterHandler (GameAwake);
            //监听服务器关闭成功事件
            //ModEvents.GameShutdown.RegisterHandler (GameShutdown);
            //监听保存完假数据事件
            ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            //监听玩家spawn事件
            //ModEvents.PlayerSpawning.RegisterHandler (PlayerSpawning);
            //监听玩家断开连接事件
            //ModEvents.PlayerDisconnected.RegisterHandler (PlayerDisconnected);
            //监听玩家在世界spawn事件
            //ModEvents.PlayerSpawnedInWorld.RegisterHandler (PlayerSpawned);
            //监听玩家发送聊天信息事件
            ModEvents.ChatMessage.RegisterHandler(ChatMessage);
            //ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);

            //击杀处理
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);


        }

        private const string ANSWER =
            "     [ff0000]成[-] [ff7f00]功[-][ffff00]加[-][80ff00]载[-] [00ffff]MOD[-][0080ff]成[-][0000ff]功[-][8b00ff]响应[-]";

        public bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName,
            bool _localizeMain, List<int> _recipientEntityIds)
        {
            /*
            //监听[/hiold]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.EqualsCaseInsensitive("/hiold"))
            {
                if (_cInfo != null)
                {
                    //获取物品信息
                    Log.Out("总物品数量：{0}", ItemClass.list.Length);
                    foreach (ItemClass _item in ItemClass.list)
                    {
                        using (StreamWriter sw = new StreamWriter("C:/AllItems.txt", true))
                        {
                            BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                            //sw.WriteLine(JsonUtils.SerializeObject(_i));
                            //sw.WriteLine();
                            if (_item != null)
                            {
                                string groupInfo = "";
                                foreach (string temp in _item.Groups)
                                {
                                    groupInfo += temp + "|";
                                }

                                if (groupInfo.Length > 0)
                                {
                                    groupInfo = groupInfo.Substring(0, groupInfo.Length - 1);
                                }

                                sw.WriteLine(string.Format("{0},{1},{2},{3}", _item.Name, _item.CustomIcon, (_item.CustomIconTint.a + "|" + _item.CustomIconTint.r + "|" + _item.CustomIconTint.g + "|" + _item.CustomIconTint.b), groupInfo));
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }


                    Log.Out("Sent chat hook reply to {0}", _cInfo.playerId);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, ANSWER, "[87CEFA]交易系统：", false, null));
                }
                else
                {
                    Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                }
                return false;
            }
            */

            //监听[/hiold]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.EqualsCaseInsensitive("/sa"))
            {
                if (_cInfo != null)
                {
                    StoreItem.StoreAllItems(_cInfo);
                }
                else
                {
                    Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                }
                return false;
            }




            //监听[/pmreg]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pmcz"))
            {
                //判断是否启用了拍卖系统

                if (Auction.IsEnable)
                {
                    if (Auction.PointMode.Equals("coin"))
                    {
                        //赌场币模式
                        if (_cInfo != null)
                        {
                            //Log.Out("响应玩家的拍卖请求 {0}", _cInfo.playerId);
                            HandleCommand.handleSaveCoin(_cInfo);
                        }
                        else
                        {
                            Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                        }
                        return false;
                    }
                    else if (Auction.PointMode.Equals("point"))
                    {
                        //点券模式
                        string[] command = _msg.Split(' ');
                        //命令参数不正确
                        if (command.Length < 2)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]转存失败,格式错误,正确格式为/pmcz 数量", "[87CEFA]交易系统：", false, null));
                            return false;
                        }

                        if (_cInfo != null)
                        {
                            if (int.TryParse(command[1], out int _auctionPrice))
                            {
                                if (_auctionPrice > 0)
                                {
                                    //Log.Out("响应玩家的拍卖请求 {0}", _cInfo.playerId);
                                    TradeAction.AuctionSavePoint(_cInfo, _auctionPrice);
                                }
                                else
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]转存失败,金额错误", "[87CEFA]交易系统：", false, null));
                                    return false;
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]转存失败,金额错误", "[87CEFA]交易系统：", false, null));
                                return false;
                            }

                        }
                        else
                        {
                            Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                        }
                        return false;
                    }
                }
                else
                {
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]服务器未开启拍卖系统无法存储代币", "[87CEFA]交易系统：", false, null));
                    return false;
                }
            }




            //响应注册用户操作
            //监听[/pmreg]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pmreg"))
            {
                //判断是否启用了拍卖系统
                if (Auction.IsEnable)
                {
                    string[] command = _msg.Split(' ');
                    //命令参数不正确
                    if (command.Length < 2)
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]注册失败,格式错误,正确格式为/pmreg 密码", "[87CEFA]交易系统：", false, null));
                        return false;
                    }

                    if (_cInfo != null)
                    {
                        //Log.Out("响应玩家的拍卖请求 {0}", _cInfo.playerId);
                        HandleCommand.handleRegUser(_cInfo, command[1]);
                    }
                    else
                    {
                        Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                    }
                    return false;
                }
                else
                {
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]服务器未开启拍卖系统无法注册", "[87CEFA]交易系统：", false, null));
                    return false;
                }
            }


            //监听[/pm]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pm"))
            {
                //判断是否启用了拍卖系统
                if (Auction.IsEnable)
                {
                    string[] command = _msg.Split(' ');
                    //命令参数不正确
                    if (command.Length < 2)
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]上架拍卖失败,格式错误,正确格式为/pm 价格", "[87CEFA]交易系统：", false, null));
                        return false;
                    }

                    if (_cInfo != null)
                    {
                        //Log.Out("响应玩家的拍卖请求 {0}", _cInfo.playerId);
                        HandleCommand.handleCommand(_cInfo, command[1]);
                    }
                    else
                    {
                        Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                    }
                    return false;
                }
                else
                {
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]服务器未开启拍卖系统", "[87CEFA]交易系统：", false, null));
                    return false;
                }
            }

            return true;
        }



        private static void GameStartDone()
        {
            //执行注入

            RunTimePatch.PatchAll();

            //检查文件夹
            if (!Directory.Exists(API.ConfigPath))
            {
                Directory.CreateDirectory(API.ConfigPath);
            }
            //加载配置文件
            LoadConfig.Load();
            //加载用户配置
            LoadUserConfig.Load();
            //加载物品配置
            LoadItemConfig.Load();
            //加载任务配置
            LoadQuestAwardConfig.Load();
            //加载击杀配置
            LoadKillAwardConfig.Load();
            //开启http服务器
            HostHttpServer.handleHttpService();
        }




        private static void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            //处理玩家任务完成信息
            QuestAward.CheckUserQuest(_cInfo, _playerDataFile);
        }



        private static void EntityKilled(Entity _entity1, Entity _entity2)
        {
            KillAward.EntityKilledHandler(_entity1, _entity2);
        }


    }
}