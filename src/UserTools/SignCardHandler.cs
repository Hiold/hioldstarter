using HioldMod.src.Serialize;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HioldMod.src.UserTools
{
    class SignCardHandler
    {
        private static readonly string file = string.Format("SignCard_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static readonly string filepath = string.Format("{0}/Logs/{1}", API.ConfigPath, file);

        public static void PlayerSign(ClientInfo _cInfo)
        {

            //初始化当日签到信息
            string fileTodaySisn = string.Format("SignUser_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
            string TodaySignfilepath = string.Format("{0}/Signs/{1}", API.ConfigPath, fileTodaySisn);
            //检查文件夹
            if (!Directory.Exists(string.Format("{0}/Signs/", API.ConfigPath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Signs/", API.ConfigPath));
            }

            CheckFile(TodaySignfilepath);

            //非空校验
            if (_cInfo == null)
            {
                return;
            }
            //检查用户权限--白名单
            if (!SignCardConfig.IsEnable)
            {
                Log.Out("玩家 {0} 尝试签到，系统未开启签到已拒绝", _cInfo.playerId);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器没有开启签到功能!", "[87CEFA]签到系统", false, null));
                return;
            }
            //玩家steamid
            string steamid = _cInfo.playerId;
            string username = _cInfo.playerName;
            string group = null;

            if (ValidateSignUserFromFile(TodaySignfilepath,steamid))
            {
                Log.Out("玩家 {0} 尝试重复签到,已阻止", _cInfo.playerId);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]您已签到过了,不能重复签到!", "[87CEFA]签到系统", false, null));
                return;
            }

            try
            {
                group = SignCardConfig.playerGroup[_cInfo.playerId];
            }
            catch (Exception e)
            {
                group = "default";
                Log.Out("玩家 {0} 尝试签到，该玩家不具有分组信息使用default发放", _cInfo.playerId);
            }

            try
            {
                List<CardAwordInfo> awords = SignCardConfig.awrodCardList[group];
                foreach (CardAwordInfo aword in awords)
                {
                    if (aword.awardtype.Equals("item"))
                    {
                        //发放物品
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} {2} 签到领取了 {3}个{4}, 品质 {5} ", DateTime.Now, _cInfo.playerId, _cInfo.playerName, aword.itemcount, aword.itemname, aword.itemquality));
                            sw.Flush();
                            sw.Close();
                        }
                        World world = GameManager.Instance.World;
                        ItemValue _itemValue = new ItemValue(ItemClass.GetItem(aword.itemname, false).type, false);
                        _itemValue.Quality = aword.itemquality;
                        EntityItem entityItem = new EntityItem();
                        entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                        {
                            entityClass = EntityClass.FromString("item"),
                            id = EntityFactory.nextEntityID++,
                            itemStack = new ItemStack(_itemValue, aword.itemcount),
                            pos = world.Players.dict[_cInfo.entityId].position,
                            rot = new Vector3(20f, 0f, 20f),
                            lifetime = 60f,
                            belongsPlayerId = _cInfo.entityId
                        });
                        world.SpawnEntityInWorld(entityItem);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                        world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]成功接收了物品", "[87CEFA]签到系统", false, null));

                    }
                    else if (aword.awardtype.Equals("command"))
                    {
                        //执行指令
                        string _command = aword.command.Replace("{username}", username).Replace("{usersteamid}", steamid);
                        SdtdConsole.Instance.ExecuteSync(_command, null);
                        //发放物品
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(string.Format("{0}: {1} {2} 签到，执行了指令 {3} ", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _command));
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                //奖励发放执行完毕写入记录
                //发放物品
                using (StreamWriter sw = new StreamWriter(TodaySignfilepath, true))
                {
                    sw.WriteLine(string.Format("{0},{1},{2}", DateTime.Now, _cInfo.playerId, _cInfo.playerName));
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out("配置错误无法获取到奖励信息");
                return;
            }
        }

        //读取文件
        public static bool ValidateSignUserFromFile(string file, string steamid)
        {
            //List<string> result = new List<string>();
            bool flag = false;
            using (StreamReader sr = new StreamReader(file, Encoding.Default))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains(steamid))
                    {
                        flag = true;
                        break;
                    }
                }
                sr.Close();
            }
            return flag;
        }

        //检查文件
        public static void CheckFile(string path)
        {
            if (!File.Exists(path))
            {
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    //开始写入
                    sw.Write("");
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                }
            }
        }
    }
}
