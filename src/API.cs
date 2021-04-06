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
        public static string ConfigPath = string.Format("{0}/Mods/HioldMod_funcs/config/", GamePath);

        public void InitMod()
        {

            //������������ʼ���ɹ��¼�
            //ModEvents.GameStartDone.RegisterHandler (GameAwake);
            //�����������رճɹ��¼�
            //ModEvents.GameShutdown.RegisterHandler (GameShutdown);
            //����������������¼�
            ModEvents.SavePlayerData.RegisterHandler(SavePlayerData);
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
            ModEvents.EntityKilled.RegisterHandler(EntityKilled);


        }

        private const string ANSWER =
            "     [ff0000]��[-] [ff7f00]��[-][ffff00]��[-][80ff00]��[-] [00ffff]MOD[-][0080ff]��[-][0000ff]��[-][8b00ff]��Ӧ[-]";

        public bool ChatMessage(ClientInfo _cInfo, EChatType _type, int _senderId, string _msg, string _mainName,
            bool _localizeMain, List<int> _recipientEntityIds)
        {
            /*
            //����[/hiold]����
            if (!string.IsNullOrEmpty(_msg) && _msg.EqualsCaseInsensitive("/hiold"))
            {
                if (_cInfo != null)
                {
                    //��ȡ��Ʒ��Ϣ
                    Log.Out("����Ʒ������{0}", ItemClass.list.Length);
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, ANSWER, "[87CEFA]����ϵͳ��", false, null));
                }
                else
                {
                    Log.Error("ChatHookExample: Argument _cInfo null on message: {0}", _msg);
                }
                return false;
            }
            */

            //����[/hiold]����
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




            //����[/pmreg]����
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pmcz"))
            {
                //�ж��Ƿ�����������ϵͳ

                if (Auction.IsEnable)
                {
                    if (Auction.PointMode.Equals("coin"))
                    {
                        //�ĳ���ģʽ
                        if (_cInfo != null)
                        {
                            //Log.Out("��Ӧ��ҵ��������� {0}", _cInfo.playerId);
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
                        //��ȯģʽ
                        string[] command = _msg.Split(' ');
                        //�����������ȷ
                        if (command.Length < 2)
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]ת��ʧ��,��ʽ����,��ȷ��ʽΪ/pmcz ����", "[87CEFA]����ϵͳ��", false, null));
                            return false;
                        }

                        if (_cInfo != null)
                        {
                            if (int.TryParse(command[1], out int _auctionPrice))
                            {
                                if (_auctionPrice > 0)
                                {
                                    //Log.Out("��Ӧ��ҵ��������� {0}", _cInfo.playerId);
                                    TradeAction.AuctionSavePoint(_cInfo, _auctionPrice);
                                }
                                else
                                {
                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]ת��ʧ��,������", "[87CEFA]����ϵͳ��", false, null));
                                    return false;
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]ת��ʧ��,������", "[87CEFA]����ϵͳ��", false, null));
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]������δ��������ϵͳ�޷��洢����", "[87CEFA]����ϵͳ��", false, null));
                    return false;
                }
            }




            //��Ӧע���û�����
            //����[/pmreg]����
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pmreg"))
            {
                //�ж��Ƿ�����������ϵͳ
                if (Auction.IsEnable)
                {
                    string[] command = _msg.Split(' ');
                    //�����������ȷ
                    if (command.Length < 2)
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]ע��ʧ��,��ʽ����,��ȷ��ʽΪ/pmreg ����", "[87CEFA]����ϵͳ��", false, null));
                        return false;
                    }

                    if (_cInfo != null)
                    {
                        //Log.Out("��Ӧ��ҵ��������� {0}", _cInfo.playerId);
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]������δ��������ϵͳ�޷�ע��", "[87CEFA]����ϵͳ��", false, null));
                    return false;
                }
            }


            //����[/pm]����
            if (!string.IsNullOrEmpty(_msg) && _msg.StartsWith("/pm"))
            {
                //�ж��Ƿ�����������ϵͳ
                if (Auction.IsEnable)
                {
                    string[] command = _msg.Split(' ');
                    //�����������ȷ
                    if (command.Length < 2)
                    {
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]�ϼ�����ʧ��,��ʽ����,��ȷ��ʽΪ/pm �۸�", "[87CEFA]����ϵͳ��", false, null));
                        return false;
                    }

                    if (_cInfo != null)
                    {
                        //Log.Out("��Ӧ��ҵ��������� {0}", _cInfo.playerId);
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[ff0000]������δ��������ϵͳ", "[87CEFA]����ϵͳ��", false, null));
                    return false;
                }
            }

            return true;
        }



        private static void GameStartDone()
        {
            //ִ��ע��

            RunTimePatch.PatchAll();

            //����ļ���
            if (!Directory.Exists(API.ConfigPath))
            {
                Directory.CreateDirectory(API.ConfigPath);
            }
            //���������ļ�
            LoadConfig.Load();
            //�����û�����
            LoadUserConfig.Load();
            //������Ʒ����
            LoadItemConfig.Load();
            //������������
            LoadQuestAwardConfig.Load();
            //���ػ�ɱ����
            LoadKillAwardConfig.Load();
            //����http������
            HostHttpServer.handleHttpService();
        }




        private static void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            //����������������Ϣ
            QuestAward.CheckUserQuest(_cInfo, _playerDataFile);
        }



        private static void EntityKilled(Entity _entity1, Entity _entity2)
        {
            KillAward.EntityKilledHandler(_entity1, _entity2);
        }


    }
}