using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "Item_tooltip_addcapsule")]
    public class CapsuleDto
    {
        [XmlElement("item")]
        public CapsuleInfoDto[] item { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CapsuleInfoDto
    {
        [XmlAttribute("id")]
        public uint capsule_id { get; set; }

        [XmlElement("capsule_icon")]
        public CapsuleIconInfoDto items { get; set; }

        [XmlElement("capsule_slot")]
        public CapsuleSlotInfoDto groups { get; set; }

        [XmlElement("capsule_info")]
        public CapsuleResultInfoDto results { get; set; }

        [XmlElement("color_index")]
        public CapsuleColorInfoDto colors { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CapsuleIconInfoDto
    {
        [XmlAttribute("ID_1")]
        public uint ID1 { get; set; }

        [XmlAttribute("ID_2")]
        public uint ID2 { get; set; }

        [XmlAttribute("ID_3")]
        public uint ID3 { get; set; }

        [XmlAttribute("ID_4")]
        public uint ID4 { get; set; }

        [XmlAttribute("ID_5")]
        public uint ID5 { get; set; }

        [XmlAttribute("ID_6")]
        public uint ID6 { get; set; }

        [XmlAttribute("ID_7")]
        public uint ID7 { get; set; }

        [XmlAttribute("ID_8")]
        public uint ID8 { get; set; }

        [XmlAttribute("ID_9")]
        public string ID9 { get; set; }

        [XmlAttribute("ID_10")]
        public string ID10 { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CapsuleSlotInfoDto
    {
        [XmlAttribute("slot_1")]
        public uint slot1 { get; set; }

        [XmlAttribute("slot_2")]
        public uint slot2 { get; set; }

        [XmlAttribute("slot_3")]
        public uint slot3 { get; set; }

        [XmlAttribute("slot_4")]
        public uint slot4 { get; set; }

        [XmlAttribute("slot_5")]
        public uint slot5 { get; set; }

        [XmlAttribute("slot_6")]
        public uint slot6 { get; set; }

        [XmlAttribute("slot_7")]
        public uint slot7 { get; set; }

        [XmlAttribute("slot_8")]
        public uint slot8 { get; set; }

        [XmlAttribute("slot_9")]
        public uint slot9 { get; set; }

        [XmlAttribute("slot_10")]
        public uint slot10 { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CapsuleResultInfoDto
    {
        [XmlAttribute("effect_key_1")]
        public string effect_key1 { get; set; }

        [XmlAttribute("effect_key_2")]
        public string effect_key2 { get; set; }

        [XmlAttribute("effect_key_3")]
        public string effect_key3 { get; set; }

        [XmlAttribute("effect_key_4")]
        public string effect_key4 { get; set; }

        [XmlAttribute("effect_key_5")]
        public string effect_key5 { get; set; }

        [XmlAttribute("effect_key_6")]
        public string effect_key6 { get; set; }

        [XmlAttribute("effect_key_7")]
        public string effect_key7 { get; set; }

        [XmlAttribute("effect_key_8")]
        public string effect_key8 { get; set; }

        [XmlAttribute("effect_key_9")]
        public string effect_key9 { get; set; }

        [XmlAttribute("effect_key_10")]
        public string effect_key10 { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CapsuleColorInfoDto
    {
        [XmlAttribute("color_1")]
        public uint color1 { get; set; }

        [XmlAttribute("color_2")]
        public uint color2 { get; set; }

        [XmlAttribute("color_3")]
        public uint color3 { get; set; }

        [XmlAttribute("color_4")]
        public uint color4 { get; set; }

        [XmlAttribute("color_5")]
        public uint color5 { get; set; }

        [XmlAttribute("color_6")]
        public uint color6 { get; set; }

        [XmlAttribute("color_7")]
        public uint color7 { get; set; }

        [XmlAttribute("color_8")]
        public uint color8 { get; set; }

        [XmlAttribute("color_9")]
        public uint color9 { get; set; }

        [XmlAttribute("color_16")]
        public uint color10 { get; set; }
    }
}
