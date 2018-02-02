using System;
using BlubLib.Serialization;
using BlubLib.Serialization.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class EquipCheckDto
    {
        public EquipCheckDto()
        {
            Weapons = Array.Empty<ItemCheckDto>();
            Skill = new ItemCheckDto();
            Costumes = Array.Empty<ItemCheckDto>();
            MovementSpeed = 1100;
        }

        [BlubMember(0, typeof(FixedArraySerializer), 3)]
        public ItemCheckDto[] Weapons { get; set; }

        [BlubMember(1)]
        public ItemCheckDto Skill { get; set; }

        [BlubMember(2, typeof(FixedArraySerializer), 8)]
        public ItemCheckDto[] Costumes { get; set; }

        [BlubMember(3)]
        public uint MovementSpeed { get; set; }
    }
}
