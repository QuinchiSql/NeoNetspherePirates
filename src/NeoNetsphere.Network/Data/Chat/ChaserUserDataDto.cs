using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class ChaserUserDataDto
    {
        [BlubMember(0)]
        public float KillProbability { get; set; }

        [BlubMember(1)]
        public float Kills { get; set; }
    }
}
