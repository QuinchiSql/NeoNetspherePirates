using BlubLib.Serialization;
using ProudNetSrc.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.Chat
{
    [BlubContract]
    public class NoteContentDto
    {
        public NoteContentDto()
        {
            Message = "";
            Gift = new NoteGiftDto();
        }

        [BlubMember(0)]
        public ulong Id { get; set; }

        [BlubMember(1, typeof(StringSerializer))]
        public string Message { get; set; }

        [BlubMember(2)]
        public NoteGiftDto Gift { get; set; }

        [BlubMember(3)]
        public byte Unk1 { get; set; }

        [BlubMember(4)]
        public byte Unk2 { get; set; }
    }
}
