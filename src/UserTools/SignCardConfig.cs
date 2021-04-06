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

    }
}
