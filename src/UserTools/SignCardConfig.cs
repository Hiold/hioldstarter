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
        //奖励列表
        public static ConcurrentDictionary<string, List<CardAwordInfo>> awrodCardList = new ConcurrentDictionary<string, List<CardAwordInfo>>();
        //用户分组列表
        public static ConcurrentDictionary<string, string> playerGroup = new ConcurrentDictionary<string, string>();
        //触发奖励机制的指令
        public static string trigerCommand;

    }
}
