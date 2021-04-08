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
        public static string ConfigPath = string.Format("{0}/Mods/hiolds7dtdSignCard_funcs/config/", GamePath);

        public void InitMod()
        {

            //监听服务器初始化成功事件
            //ModEvents.GameStartDone.RegisterHandler (GameAwake);
            //监听服务器关闭成功事件
            //ModEvents.GameShutdown.RegisterHandler (GameShutdown);
            //监听保存完假数据事件
            //ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
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
            //ModEvents.EntityKilled.RegisterHandler(EntityKilled);


        }

        public bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName,
            bool _localizeMain, List<int> _recipientEntityIds)
        {
            //监听[/hiold]命令
            if (!string.IsNullOrEmpty(_msg) && _msg.EqualsCaseInsensitive(SignCardConfig.trigerCommand))
            {
                if (_cInfo != null)
                {
                    SignCardHandler.PlayerSign(_cInfo);
                }
                else
                {
                    Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                }
                return false;
            }

            return true;
        }

        //游戏加载完毕触发事件
        private static void GameStartDone()
        {
            //检查文件夹
            if (!Directory.Exists(API.ConfigPath))
            {
                Directory.CreateDirectory(API.ConfigPath);
            }
            //加载主配置信息
            LoadSignCardMainConfig.Load();
            //加载奖励配置信息
            LoadSignCardInfoConfig.Load();
            //加载玩家配置信息
            LoadSignCardPlayerConfig.Load();

            //检查文件夹
            if (!Directory.Exists(string.Format("{0}/Logs/", API.ConfigPath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Logs/", API.ConfigPath));
            }

        }
    }
}