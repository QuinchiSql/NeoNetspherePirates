namespace NeoNetsphere
{
    internal class PlayerLocationInfo
    {
        public PlayerLocationInfo()
        {
        }

        public PlayerLocationInfo(int _id)
        {
            channelid = _id;
        }

        public int channelid { get; set; }
        public bool invisible { get; set; } = false;
    }
}
