namespace HioldMod.src.UserTools
{
    class HandleCommand
    {
        public static void handleCommand(ClientInfo _cInfo, string price)
        {
            //EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            RemoveItem.CheckBox(_cInfo, price);
        }

        public static void handleRegUser(ClientInfo _cInfo, string password)
        {
            //EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            UserRegAction.RegNewUser(_cInfo, password);
        }

        public static void handleSaveCoin(ClientInfo _cInfo)
        {
            //EntityPlayer _player = GameManager.Instance.World.Players.dict[_cInfo.entityId];
            RemovecasinoCoin.CheckCoin(_cInfo);
        }
    }
}
