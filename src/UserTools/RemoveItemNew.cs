using ServerTools;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HioldMod.src.UserTools
{
    class RemoveItemNew
    {
        private static System.Random random = new System.Random();
        private static readonly string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static readonly string filepath = string.Format("{0}/Logs/{1}", API.ConfigPath, file);

        public static void CheckBoxNew(ClientInfo _cInfo, string _price)
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
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖白名单,您不在白名单中,不允许拍卖", "[87CEFA]交易系统：", false, null));
                    return;
                }
            }



            //检查用户权限--黑名单
            if (Auction.UserContorlMode.Equals("BlackList"))
            {
                if (Auction.UserContorlList.Contains(_cInfo.playerId))
                {
                    Log.Out("玩家 {0} 在黑名单中,不允许操作", _cInfo.playerId);
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖黑名单,您在黑名单列表中,不允许拍卖", "[87CEFA]交易系统：", false, null));
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
                        if (int.TryParse(_price, out int _auctionPrice))
                        {
                            if (_auctionPrice > 0)
                            {

                                //检测玩家附近是否有其他玩家
                                DictionaryList<int, EntityPlayer> players = GameManager.Instance.World.Players;
                                foreach (EntityPlayer _tplayer in players.dict.Values)
                                {
                                    //首先排除自己
                                    if (_tplayer.entityId != _player.entityId)
                                    {
                                        if ((_tplayer.position.x - _player.position.x) * (_tplayer.position.x - _player.position.x) + (_tplayer.position.z - _player.position.z) * (_tplayer.position.z - _player.position.z) <= 10 * 10)
                                        {
                                            if (_tplayer.position.y >= (int)_player.position.y - 10 && _tplayer.position.y <= (int)_player.position.y + 10)
                                            {
                                                Log.Out("玩家 {0}，{1} 距离太近执行拍卖---已阻止", _player.entityId, _tplayer.entityId);
                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]检测到您附近有其他玩家，无法拍卖，请确认周围环境", "[87CEFA]交易系统：", false, null));
                                                return;
                                            }
                                        }
                                    }
                                }







                                LinkedList<Chunk> _chunkArray = new LinkedList<Chunk>();
                                DictionaryList<Vector3i, TileEntity> _tiles = new DictionaryList<Vector3i, TileEntity>();
                                ChunkClusterList _chunklist = GameManager.Instance.World.ChunkClusters;
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
                                                        Log.Out("检测到容器IsUserAccessing:" + _tile.IsUserAccessing());

                                                        if (SecureLoot.IsUserAllowed(_cInfo.playerId) && !SecureLoot.IsUserAccessing())
                                                        {
                                                            ItemStack[] items = SecureLoot.items;
                                                            ItemStack _item = items[0];
                                                            if (_item != null && !_item.IsEmpty())
                                                            {

                                                                //检查箱子是否可用
                                                                if (_tile.IsUserAccessing())
                                                                {
                                                                    Log.Out("玩家 {0} 尝试从有密码的箱子中拍卖物品---已阻止", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]检测到容器被设置了密码，无法拍卖", "[87CEFA]交易系统：", false, null));
                                                                    return;
                                                                }

                                                                if (!SecureLoot.IsLocked())
                                                                {
                                                                    Log.Out("玩家 {0} 尝试从未上锁的箱子中拍卖物品---已阻止", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]检测到容器已解锁，无法拍卖", "[87CEFA]交易系统：", false, null));
                                                                    return;
                                                                }


                                                                //检查物品黑白名单设置
                                                                //检查物品--白名单
                                                                if (Auction.ItemContorlMode.Equals("WhiteList"))
                                                                {
                                                                    if (Auction.ItemContorlList.Contains(_item.itemValue.ItemClass.GetItemName()))
                                                                    {
                                                                        Log.Out("玩家 {0} 拍卖物品 {1},物品为白名单,允许操作", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                    }
                                                                    else
                                                                    {
                                                                        Log.Out("玩家 {0} 拍卖物品 {1},物品非白名单,不用允许操作", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖物品白名单,物品" + _item.itemValue.ItemClass.GetItemName() + "不在白名单中,不允许拍卖", "[87CEFA]交易系统：", false, null));
                                                                        return;
                                                                    }
                                                                }



                                                                //检查物品--黑名单
                                                                if (Auction.ItemContorlMode.Equals("BlackList"))
                                                                {
                                                                    if (Auction.ItemContorlList.Contains(_item.itemValue.ItemClass.GetItemName()))
                                                                    {
                                                                        Log.Out("玩家 {0} 拍卖物品 {1},物品为黑名单,不允许操作", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]服务器启用了拍卖物品黑名单,物品" + _item.itemValue.ItemClass.GetItemName() + "在黑名单中,不允许拍卖", "[87CEFA]交易系统：", false, null));
                                                                        return;
                                                                    }
                                                                    else
                                                                    {
                                                                        Log.Out("玩家 {0} 拍卖物品 {1},物品非黑名单,允许操作", _cInfo.playerId, _item.itemValue.ItemClass.GetItemName());
                                                                    }
                                                                }


                                                                int _id = GenerateAuctionId();
                                                                if (_id > 0)
                                                                {

                                                                    ItemDataSerializable _serializedItemStack = new ItemDataSerializable();
                                                                    
                                                                    //调用拍卖管理系统
                                                                    bool isManageMentOk = TradeAction.AuctionAction(_cInfo, _serializedItemStack, _price, _id);
                                                                    if (isManageMentOk)
                                                                    {
                                                                        Log.Out("拍卖系统调用成功");
                                                                    }
                                                                    else
                                                                    {
                                                                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败:系统内部异常,请联系管理员", "[87CEFA]交易系统：", false, null));
                                                                        return;
                                                                    }

                                                                    items[0] = ItemStack.Empty.Clone();
                                                                    _tile.SetModified();
                                                                    using (StreamWriter sw = new StreamWriter(filepath, true))
                                                                    {
                                                                        sw.WriteLine(string.Format("{0}: {1} {2} 上架 {3}个{4}, {5} 品质, %{6} 耐久度--价格：{7}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count, _item.itemValue.ItemClass.GetItemName(), _item.itemValue.Quality, _item.itemValue.UseTimes / _item.itemValue.MaxUseTimes * 100, _price));
                                                                        //sw.WriteLine();
                                                                        sw.Flush();
                                                                        sw.Close();
                                                                    }
                                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]成功上架拍卖" + _item.itemValue.ItemClass.GetLocalizedItemName() ?? _item.itemValue.ItemClass.GetItemName(), "[87CEFA]交易系统：", false, null));
                                                                    return;
                                                                }
                                                                else
                                                                {
                                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败ID错误", "[87CEFA]交易系统：", false, null));
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败容器中第一格为空", "[87CEFA]交易系统：", false, null));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败您没有此容器权限", "[87CEFA]交易系统：", false, null));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]上架拍卖失败,价格不能为小于0", "[87CEFA]交易系统：", false, null));
                            }
                        }
                        else
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[00FF00]上架拍卖失败,格式错误正确格式为/pm 价格", "[87CEFA]交易系统：", false, null));
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
