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

        public string itemname;
        public string itemquality;
        public string itemcount;

        public CardAwordInfo()
        {
        }

        public CardAwordInfo(string itemname, string itemquality, string itemcount)
        {
            this.itemname = itemname;
            this.itemquality = itemquality;
            this.itemcount = itemcount;
        }
    }
}
