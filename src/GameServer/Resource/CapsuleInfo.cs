using System;
using System.Collections.Generic;

namespace NeoNetsphere.Resource
{
    public class CapsuleInfo
    {
        public CapsuleInfo(uint id, Dictionary<uint, Tuple<string, uint>> info)
        {
            CapsuleId = id;
            Iteminfos = info;
        }

        public uint CapsuleId { get; set; }
        public Dictionary<uint, Tuple<string, uint>> Iteminfos { get; set; } //group, resultinfo, color
    }
    
}
