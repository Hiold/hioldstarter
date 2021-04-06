using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HioldMod.src.UserTools
{
    class Auction
    {
        //是否开启
        public static bool IsEnable;
        //用户控制模式
        public static string UserContorlMode;
        //用户列表
        public static List<string> UserContorlList = new List<string>();
        //物品控制模式
        public static string ItemContorlMode;
        //物品列表
        public static List<string> ItemContorlList = new List<string>();
        //服务器地址
        public static string ServerHost;
        //服务器Key
        public static string ServerKey;
        //本地服务端口
        public static string Port;
        //货币模式
        public static string PointMode;

        //已打开箱子列表
        public static ConcurrentDictionary<string, Vector3i> opendLootList = new ConcurrentDictionary<string, Vector3i>();
    }
}
