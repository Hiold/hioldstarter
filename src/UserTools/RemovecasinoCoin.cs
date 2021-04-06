using ServerTools;
using System;
using System.Collections.Generic;
using System.IO;

namespace HioldMod.src.UserTools
{
    class RemovecasinoCoin
    {
        private static System.Random random = new System.Random();
        private static readonly string file = string.Format("chongzhi_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static readonly string filepath = string.Format("{0}/Logs/{1}", API.ConfigPath, file);

        public static void CheckCoin(ClientInfo _cInfo)
        {
            //检查用户权限--白名单
            if (Auction.UserContorlMode.Equals("WhiteList"))
            {
                if (Auction.UserContorlList.Contains(_cInfo.playerId))
                {
                    Log.Out("玩家 {0} 拥有拍卖白名单,允许操作", _cInfo.playerId);
                }
                else
                {
                    Log.Out("玩家 {0} 没有拍卖白名单,不允许操作", _cInfo.playerId);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖白名单,您不在白名单中,不允许此操作", "[87CEFA]交易系统：", false, null));
                    return;
                }
            }



            //检查用户权限--黑名单
            if (Auction.UserContorlMode.Equals("BlackList"))
            {
                if (Auction.UserContorlList.Contains(_cInfo.playerId))
                {
                    Log.Out("玩家 {0} 在黑名单中,不允许操作", _cInfo.playerId);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖黑名单,您在黑名单列表中,不允许许此操作", "[87CEFA]交易系统：", false, null));
                    return;
                }
                else
                {
                    Log.Out("玩家 {0} 不在黑名单中,允许操作", _cInfo.playerId);
                }
            }




            //检查文件夹
            if (!Directory.Exists(string.Format("{0}/Logs/", API.ConfigPath)))
            {
                Directory.CreateDirectory(string.Format("{0}/Logs/", API.ConfigPath));
            }
            try
            {
                if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
                {
                    EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                    if (_player != null)
                    {

                        LinkedList<Chunk> _chunkArray = new LinkedList<Chunk>();
                        DictionaryList<Vector3i, TileEntity> _tiles = new DictionaryList<Vector3i, TileEntity>();
                        ChunkClusterList _chunklist = GameManager.Instance.World.ChunkClusters;
                        int count = 0;




                        for (int i = 0; i < _chunklist.Count; i++)
                        {
                            ChunkCluster _chunk = _chunklist[i];
                            _chunkArray = _chunk.GetChunkArray();
                            foreach (Chunk _c in _chunkArray)
                            {
                                _tiles = _c.GetTileEntities();
                                foreach (TileEntity _tile in _tiles.dict.Values)
                                {
                                    TileEntityType _type = _tile.GetTileEntityType();
                                    if (_type.ToString().Equals("SecureLoot"))
                                    {
                                        TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)_tile;
                                        Vector3i vec3i = SecureLoot.ToWorldPos();
                                        //检测附近箱子数量
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 3 * 3)
                                        {
                                            if (vec3i.y >= (int)_player.position.y - 3 && vec3i.y <= (int)_player.position.y + 3)
                                            {
                                                if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                                {
                                                    count++;
                                                }
                                            }
                                        }

                                        //附近箱子数量为0
                                        if (count == 0)
                                        {
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]操作失败附近没有容器", "[87CEFA]交易系统：", false, null));
                                            return;

                                        }
                                        //附近有多个箱子
                                        else if (count > 1)
                                        {
                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]操作失败附近有多个容器,请确保您附近只有一个容器", "[87CEFA]交易系统：", false, null));
                                            return;
                                        }
                                    }
                                }
                            }
                        }





                        //校验通过
                        for (int i = 0; i < _chunklist.Count; i++)
                        {
                            ChunkCluster _chunk = _chunklist[i];
                            _chunkArray = _chunk.GetChunkArray();
                            foreach (Chunk _c in _chunkArray)
                            {
                                _tiles = _c.GetTileEntities();
                                foreach (TileEntity _tile in _tiles.dict.Values)
                                {
                                    TileEntityType _type = _tile.GetTileEntityType();
                                    if (_type.ToString().Equals("SecureLoot"))
                                    {
                                        TileEntitySecureLootContainer SecureLoot = (TileEntitySecureLootContainer)_tile;
                                        Vector3i vec3i = SecureLoot.ToWorldPos();
                                        if ((vec3i.x - _player.position.x) * (vec3i.x - _player.position.x) + (vec3i.z - _player.position.z) * (vec3i.z - _player.position.z) <= 3 * 3)
                                        {
                                            if (vec3i.y >= (int)_player.position.y - 3 && vec3i.y <= (int)_player.position.y + 3)
                                            {
                                                if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                                {
                                                    ItemStack[] items = SecureLoot.items;
                                                    ItemStack _item = items[0];
                                                    if (_item != null && !_item.IsEmpty())
                                                    {
                                                        int _id = GenerateAuctionId();
                                                        if (_id > 0)
                                                        {
                                                            
                                                            ItemDataSerializable _serializedItemStack = new ItemDataSerializable();
                                                            
                                                            //检查物品类型
                                                            if (!"casinoCoin".Equals(_item.itemValue.ItemClass.GetItemName()))
                                                            {
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]存放失败:此物品非赌场代币", "[87CEFA]交易系统：", false, null));
                                                                return;
                                                            }


                                                            //调用拍卖管理系统
                                                            bool isManageMentOk = TradeAction.AuctionSaveCoin(_cInfo, _serializedItemStack, _id);
                                                            if (isManageMentOk)
                                                            {
                                                                Log.Out("拍卖系统调用成功");
                                                            }
                                                            else
                                                            {
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]存放失败:系统内部异常,请联系管理员", "[87CEFA]交易系统：", false, null));
                                                                return;
                                                            }

                                                            items[0] = ItemStack.Empty.Clone();
                                                            _tile.SetModified();
                                                            using (StreamWriter sw = new StreamWriter(filepath, true))
                                                            {
                                                                sw.WriteLine(string.Format("{0}: {1} {2} 存放 {3} 个赌场币进交易系统", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count));
                                                                //sw.WriteLine();
                                                                sw.Flush();
                                                                sw.Close();
                                                            }
                                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]成功存放赌场币进交易系统" + _item.itemValue.ItemClass.GetLocalizedItemName() ?? _item.itemValue.ItemClass.GetItemName(), "[87CEFA]交易系统：", false, null));
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]操作失败ID错误", "[87CEFA]交易系统：", false, null));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]操作失败容器中第一格为空", "[87CEFA]交易系统：", false, null));
                                                    }
                                                }
                                                else
                                                {
                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]操作失败您没有此容器权限", "[87CEFA]交易系统：", false, null));
                                                }
                                            }
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
                Log.Out(string.Format("[HioldMod] Error in AuctionBox.CheckBox: {0}", e.Message));
            }
        }
        private static int GenerateAuctionId()
        {
            int _id = random.Next(1, Int32.MaxValue);
            return _id;
        }
    }
}
