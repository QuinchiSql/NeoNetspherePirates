using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class ScoreDto
    {
        public ScoreDto()
        {
            Killer = 0;
            Target = 0;
        }

        public ScoreDto(LongPeerId killer, LongPeerId target, AttackAttribute weapon)
        {
            Killer = killer;
            Target = target;
            Weapon = weapon;
        }

        [BlubMember(0)]
        public LongPeerId Killer { get; set; }

        [BlubMember(1, typeof(EnumSerializer), typeof(int))]
        public AttackAttribute Weapon { get; set; }

        [BlubMember(2)]
        public LongPeerId Target { get; set; }

        [BlubMember(3)]
        public byte Unk { get; set; }
    }

    [BlubContract]
    public class Score2Dto
    {
        public Score2Dto()
        {
            Killer = 0;
            Target = 0;
        }

        public Score2Dto(LongPeerId killer, LongPeerId target, AttackAttribute weapon)
        {
            Killer = killer;
            Target = target;
            Weapon = weapon;
        }

        [BlubMember(0)]
        public LongPeerId Killer { get; set; }

        [BlubMember(1, typeof(EnumSerializer), typeof(int))]
        public AttackAttribute Weapon { get; set; }

        [BlubMember(2)]
        public LongPeerId Target { get; set; }
    }
}
