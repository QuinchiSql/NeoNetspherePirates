using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "S4_Items")]
    public class ItemInfoDto_3
    {
        [XmlElement("Item")] public ItemInfoInfoDto_3[] Item { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ItemInfoInfoDto_3
    {
        [XmlAttribute("ID")] public uint ID { get; set; }

        [XmlAttribute("Color_Count")] public uint Color_Count { get; set; }

        [XmlAttribute("Name")] public string Name { get; set; }

        [XmlAttribute("Description")] public string Description { get; set; }
    }
}
