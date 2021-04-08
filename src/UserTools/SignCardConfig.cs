using HioldMod.src.Serialize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HioldMod.src.UserTools
{
    public class SignCardConfig
    {
        //配置项
        //是否开启
        public static bool IsEnable;
        //奖励信息
        public static ConcurrentDictionary<string, List<List<CardAwordInfo>>> awrodCardList = new ConcurrentDictionary<string, List<List<CardAwordInfo>>>();
        //用户分组列表
        public static ConcurrentDictionary<string, PlayerGroup> playerGroup = new ConcurrentDictionary<string, PlayerGroup>();
        //触发奖励机制的指令
        public static string trigerCommand;

    }

    public class PlayerGroup
    {
        public string group { get; set; }
        public string type { get; set; }
    }
}
