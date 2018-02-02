using BlubLib.Serialization;

namespace ProudNetSrc.Serialization
{
    [BlubContract]
    internal class RelayDestinationDto
    {
        public RelayDestinationDto()
        {
        }

        public RelayDestinationDto(uint hostId, uint frameNumber)
        {
            HostId = hostId;
            FrameNumber = frameNumber;
        }

        [BlubMember(0)]
        public uint HostId { get; set; }

        [BlubMember(1)]
        public uint FrameNumber { get; set; }
    }
}
