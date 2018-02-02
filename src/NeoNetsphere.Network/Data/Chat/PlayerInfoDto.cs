using BlubLib.Serialization;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class PlayerInfoDto
    {
        public PlayerInfoDto()
        {
            Info = new PlayerInfoShortDto();
            Location = new PlayerLocationDto();
        }

        public PlayerInfoDto(PlayerInfoShortDto info, PlayerLocationDto location)
        {
            Info = info;
            Location = location;
        }

        [BlubMember(0)]
        public PlayerInfoShortDto Info { get; set; }

        [BlubMember(1)]
        public PlayerLocationDto Location { get; set; }
    }
}
