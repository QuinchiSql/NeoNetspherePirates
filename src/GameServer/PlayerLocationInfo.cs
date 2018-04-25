namespace NeoNetsphere
{
    internal class PlayerLocationInfo
    {
        public PlayerLocationInfo()
        {
        }

        public PlayerLocationInfo(int _id)
        {
            Channelid = _id;
        }

        public int Channelid { get; set; }
        public bool Invisible { get; set; } = false;
    }
}
