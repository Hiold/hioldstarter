using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HioldMod.src.UserTools
{
    class ClientInfoUtils
    {
        public static ClientInfo GetClientInfoFromEntityId(int _playerId)
        {
            ClientInfo _cInfo = ConnectionManager.Instance.Clients.ForEntityId(_playerId);
            if (_cInfo != null)
            {
                return _cInfo;
            }
            return null;
        }
    }
}
