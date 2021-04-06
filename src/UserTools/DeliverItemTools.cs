using HioldMod.src.Serialize;
using ServerTools;
using System;
using System.IO;
using UnityEngine;

namespace HioldMod.src.UserTools
{
    class DeliverItemTools
    {
        private static readonly string file = string.Format("wupinfafang_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static readonly string filepath = string.Format("{0}/Logs/{1}", API.ConfigPath, file);

        public static string deliverItem(string steamid, string itemData, string count)
        {

            //获取客户端信息
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForPlayerId(steamid);

            if (_cInfo == null)
            {
                Log.Out("玩家 " + steamid + " 不在线");
                return "提取失败,玩家不在线";
            }


            if (!Auction.IsEnable)
            {
                Log.Out("玩家 {0} 提取物品，服务器未开启拍卖系统，无法发放", _cInfo.playerId);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器未开启拍卖系统，物品发放失败！", "[87CEFA]交易系统", false, null));
                return "物品为黑名单,不允许操作";
            }


            //检查文件夹
            if (!Directory.Exists(string.Format("{0}/Logs/", API.ConfigPath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Logs/", API.ConfigPath));
            }


            ItemStack[] itemStacks = JsonUtils.ItemFromString(itemData);
            //发放物品
            if (itemStacks != null && itemStacks.Length > 0)
            {

                var _itemStack = itemStacks[0];

                using (StreamWriter sw = new StreamWriter(filepath, true))
                {
                    sw.WriteLine(string.Format("{0}: {1} {2} 提取 {3}个{4}, {5} 品质, {6} 耐久度.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _itemStack.count, _itemStack.itemValue.ItemClass.Name, _itemStack.itemValue.Quality, _itemStack.itemValue.UseTimes));
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }





                World world = GameManager.Instance.World;
                //根据客户端提供数量修改对应数量
                var prepireStack = itemStacks[0];
                if (int.TryParse(count, out int tmpcount))
                {
                    prepireStack.count = tmpcount;
                }
                //执行发放物品
                EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    itemStack = prepireStack,
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
            }


            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]成功接收了物品", "[87CEFA]交易系统：", false, null));


            return "success";
        }

    }
}
