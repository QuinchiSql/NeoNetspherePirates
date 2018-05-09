using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "ItemReward")]
    public class ItemRewardDto
    {
        [XmlElement("item")]
        public ItemDto[] Items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemDto
    {
        [XmlAttribute]
        public uint Number { get; set; }

        [XmlElement("group")]
        public RewardGroup[] Groups { get; set; }
    }

    [XmlType(AnonymousType =true)]
    public class RewardGroup
    {
        [XmlElement("reward")]
        public RewardDto[] Rewards { get; set; }
    }

    [XmlType(AnonymousType =true)]
    public class RewardDto
    {
        // 1: PEN
        // 2: Item
        [XmlAttribute]
        public uint Type { get; set; }

        // itemNumber
        [XmlAttribute]
        public uint Data { get; set; } = 0;

        // PriceType
        // Pen, Ap, Premium, Cp
        [XmlAttribute]
        public uint PriceType { get; set; } = 0;

        // PeriodType
        // Permanent, Day, Hours, Unity
        [XmlAttribute]
        public uint PeriodType { get; set; } = 0;

        /// <summary>
        /// Gets or Set Color of item
        /// </summary>
        [XmlAttribute]
        public byte Color { get; set; } = 0;

        // Period or Pen
        [XmlAttribute]
        public uint Value { get; set; }

        [XmlAttribute]
        public string Effects { get; set; } = "0";

        [XmlAttribute]
        public uint Rate { get; set; }
    }
}
