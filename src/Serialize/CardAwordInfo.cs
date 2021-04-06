using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HioldMod.src.Serialize
{
    public class CardAwordInfo
    {
        public string awardtype;
        public string command;
        public string itemname;
        public int itemquality;
        public int itemcount;

        public CardAwordInfo()
        {
        }

        public CardAwordInfo(string awardtype, string command, string itemname, int itemquality, int itemcount)
        {
            this.awardtype = awardtype;
            this.command = command;
            this.itemname = itemname;
            this.itemquality = itemquality;
            this.itemcount = itemcount;
        }
    }
}
