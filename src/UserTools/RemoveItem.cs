using HioldMod.src.Serialize;
using ServerTools;
using System;
using System.Collections.Generic;
using System.IO;

namespace HioldMod.src.UserTools
{
    class RemoveItem
    {
        private static System.Random random = new System.Random();
        private static readonly string file = string.Format("Auction_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static readonly string filepath = string.Format("{0}/Logs/{1}", API.ConfigPath, file);

        public static void CheckBox(ClientInfo _cInfo, string _price)
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

            if (GameManager.Instance.World.Players.dict.ContainsKey(_cInfo.entityId))
            {
                EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
                if (_player != null)
                {
                    if (int.TryParse(_price, out int _auctionPrice))
                    {
                        if (_auctionPrice > 0)
                        {
                            TileEntitySecureLootContainer tarGetSecureLoot;
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
                                        }
                                    }
                                }
                            }



                            //附近箱子数量为0
                            if (count == 0)
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败附近没有容器", "[87CEFA]交易系统：", false, null));
                                return;

                            }
                            //附近有多个箱子
                            else if (count > 1)
                            {
                                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败附近有多个容器,请确保您附近只有一个容器", "[87CEFA]交易系统：", false, null));
                                return;
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
                                                            //检查物品黑白名单设置





                                                            try
                                                            {
                                                                Vector3i tmp = Auction.opendLootList[vec3i.x + vec3i.y + vec3i.z + ""];
                                                                if (tmp != null)
                                                                {
                                                                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]有玩家打开了此容器,无法继续拍卖", "[87CEFA]交易系统：", false, null));
                                                                    Log.Out(string.Format("[FF0000]有玩家打开容器,无法继续拍卖"));
                                                                    return;
                                                                }
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                Log.Out(string.Format("[FF0000]没有玩家打开容器----继续拍卖"));
                                                            }






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
                                                                _serializedItemStack.price = _auctionPrice;

                                                                _serializedItemStack.name = _cInfo.playerName;

                                                                _serializedItemStack.steamid = _cInfo.playerId;

                                                                _serializedItemStack.tradeid = _id + "";

                                                                if (_item.itemValue.ItemClass.CustomIcon != null)
                                                                {
                                                                    _serializedItemStack.CustomIcon = _item.itemValue.ItemClass.CustomIcon.Value;


                                                                }

                                                                if (_item.itemValue.ItemClass.CustomIconTint != null)
                                                                {
                                                                    var color = _item.itemValue.ItemClass.CustomIconTint;
                                                                    _serializedItemStack.CustomIconTint = color.a + "," + color.r + "," + color.g + "," + color.b;

                                                                }

                                                                _serializedItemStack.itemCount = _item.count + "";

                                                                _serializedItemStack.itemName = _item.itemValue.ItemClass.GetItemName();

                                                                _serializedItemStack.itemUseTime = _item.itemValue.UseTimes + "";

                                                                _serializedItemStack.itemQuality = _item.itemValue.Quality + "";

                                                                _serializedItemStack.itemMaxUseTime = _item.itemValue.MaxUseTimes + "";

                                                                ItemStack[] wtItem = new ItemStack[1];
                                                                wtItem[0] = _item;
                                                                var itemString = JsonUtils.ByteStringFromItem(wtItem);
                                                                _serializedItemStack.itemData = itemString;
                                                                //物品内模组信息
                                                                List<string> modList = new List<string>();
                                                                foreach (ItemValue mod in _item.itemValue.Modifications)
                                                                {
                                                                    if (mod.ItemClass != null)
                                                                    {
                                                                        modList.Add(mod.ItemClass.GetItemName());
                                                                    }
                                                                }


                                                                //物品Groups信息
                                                                List<string> GroupList = new List<string>();
                                                                foreach (string group in _item.itemValue.ItemClass.Groups)
                                                                {
                                                                    GroupList.Add(group);
                                                                }
                                                                _serializedItemStack.Groups = GroupList;
                                                                _serializedItemStack.Modifications = modList;




                                                                //调用拍卖管理系统
                                                                bool isManageMentOk = TradeAction.AuctionActionNew(_serializedItemStack, _cInfo);
                                                                if (isManageMentOk)
                                                                {
                                                                    //Log.Out("拍卖系统调用成功");
                                                                }
                                                                else
                                                                {
                                                                    //_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "[FF0000]上架失败:请检查是否已注册", "[87CEFA]交易系统：", false, null));
                                                                    //暂时禁用
                                                                    Log.Out("未成功上架");
                                                                    return;
                                                                }

                                                                items[0] = ItemStack.Empty.Clone();
                                                                _tile.SetModified();
                                                                using (StreamWriter sw = new StreamWriter(filepath, true))
                                                                {
                                                                    BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                                                                    GameUtils.WriteItemStack(bw, items);
                                                                    //sw.WriteLine(JsonUtils.SerializeObject(_i));
                                                                    //sw.WriteLine();
                                                                    sw.WriteLine(string.Format("{0}: {1} {2} 上架 {3}个{4}, {5} 品质, %{6} 耐久度--价格：{7}.", DateTime.Now, _cInfo.playerId, _cInfo.playerName, _item.count, _item.itemValue.ItemClass.GetItemName(), _item.itemValue.Quality, _item.itemValue.UseTimes / _item.itemValue.MaxUseTimes * 100, _price));
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
        private static int GenerateAuctionId()
        {
            int _id = random.Next(1, Int32.MaxValue);
            return _id;
        }
    }
}
