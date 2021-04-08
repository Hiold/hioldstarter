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
        //��Ϸ·��
        public static string GamePath = Directory.GetCurrentDirectory();
        //config·��
        public static string ConfigPath = string.Format("{0}/Mods/hiolds7dtdSignCard_funcs/config/", GamePath);

        public void InitMod()
        {

            //������������ʼ���ɹ��¼�
            //ModEvents.GameStartDone.RegisterHandler (GameAwake);
            //�����������رճɹ��¼�
            //ModEvents.GameShutdown.RegisterHandler (GameShutdown);
            //����������������¼�
            //ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            //�������spawn�¼�
            //ModEvents.PlayerSpawning.RegisterHandler (PlayerSpawning);
            //������ҶϿ������¼�
            //ModEvents.PlayerDisconnected.RegisterHandler (PlayerDisconnected);
            //�������������spawn�¼�
            //ModEvents.PlayerSpawnedInWorld.RegisterHandler (PlayerSpawned);
            //������ҷ���������Ϣ�¼�
            ModEvents.ChatMessage.RegisterHandler(ChatMessage);
            //ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
            ModEvents.GameStartDone.RegisterHandler(GameStartDone);

            //��ɱ����
            //ModEvents.EntityKilled.RegisterHandler(EntityKilled);


        }

        public bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName,
            bool _localizeMain, List<int> _recipientEntityIds)
        {
            //����[/hiold]����
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

        //��Ϸ������ϴ����¼�
        private static void GameStartDone()
        {
            //����ļ���
            if (!Directory.Exists(API.ConfigPath))
            {
                Directory.CreateDirectory(API.ConfigPath);
            }
            //������������Ϣ
            LoadSignCardMainConfig.Load();
            //���ؽ���������Ϣ
            LoadSignCardInfoConfig.Load();
            //�������������Ϣ
            LoadSignCardPlayerConfig.Load();

            //����ļ���
            if (!Directory.Exists(string.Format("{0}/Logs/", API.ConfigPath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Logs/", API.ConfigPath));
            }

        }
    }
}