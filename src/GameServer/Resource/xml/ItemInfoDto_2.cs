using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "itemlist")]
    public class ItemInfoDto_2
    {
        [XmlElement("item")] public ItemInfoInfoDto_2[] item { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemInfoInfoDto_2
    {
        [XmlAttribute("item_key")] public uint item_key { get; set; }

        [XmlElement("base")] public ItemBaseInfoDto_2 Base { get; set; }

        [XmlElement("graphic")] public ItemGraphicInfoDto_2 graphic { get; set; }

        [XmlElement("esperchip")] public ItemEsperChipInfoDto_2 esperchip { get; set; }

        [XmlElement("etc")] public ItemEtcInfoDto_2 etc { get; set; }

        [XmlElement("sequence")] public ItemSequenceInfoDto_2 sequence { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemBaseInfoDto_2
    {
        [XmlAttribute("name")] public string name { get; set; }

        [XmlAttribute("name_key")] public string name_key { get; set; }

        [XmlAttribute("attrib_comment_key")] public string attrib_comment_key { get; set; }

        [XmlAttribute("sex")] public string sex { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemGraphicInfoDto_2
    {
        [XmlAttribute("icon_image")] public string icon_image { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemEsperChipInfoDto_2
    {
    }

    [XmlType(AnonymousType = true)]
    public class ItemEtcInfoDto_2
    {
    }

    [XmlType(AnonymousType = true)]
    public class ItemSequenceInfoDto_2
    {
    }
}
