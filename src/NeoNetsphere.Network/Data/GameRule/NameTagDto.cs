using System;
using BlubLib.Serialization;
using NeoNetsphere.Network.Serializers;

namespace NeoNetsphere.Network.Data.GameRule
{
    [BlubContract]
    public class NameTagDto
    {
        public NameTagDto()
        {
            AccountId = 0;
            TagId = 0;
        }

        public NameTagDto(ulong accountId, uint tagId)
        {
            AccountId = accountId;
            TagId = tagId;
        }
        
        [BlubMember(0)]
        public ulong AccountId { get; set; } 

        [BlubMember(1)]
        public uint TagId { get; set; } 
    }
}
