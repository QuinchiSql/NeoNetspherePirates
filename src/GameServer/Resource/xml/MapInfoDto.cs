using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "maplist")]
    public class MapInfoDto
    {
        [XmlElement("map")] public MapInfoInfoDto[] map { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class MapInfoInfoDto
    {
        [XmlAttribute("id")] public int id { get; set; }

        [XmlElement("base")] public BaseMapInfoDto Base { get; set; }

        [XmlElement("resourse")] public ResourceMapInfoDto resource { get; set; }

        [XmlElement("switch")] public SwitchMapInfoDto Switch { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class BaseMapInfoDto
    {
        [XmlAttribute("map_name_key")] public string map_name_key { get; set; }

        [XmlAttribute("mode_number")] public int mode_number { get; set; }

        [XmlAttribute("limit_player")] public int limit_player { get; set; }

        [XmlAttribute("index_number")] public int index_number { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class ResourceMapInfoDto
    {
        [XmlAttribute("bginfo_path")] public string bginfo_path { get; set; }

        [XmlAttribute("previewinfo_path")] public string previewinfo_path { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class SwitchMapInfoDto
    {
        [XmlAttribute] public string kr { get; set; }

        [XmlAttribute] public string eu { get; set; }

        [XmlAttribute] public string cn { get; set; }

        [XmlAttribute] public string th { get; set; }

        [XmlAttribute] public string tw { get; set; }

        [XmlAttribute] public string jp { get; set; }

        [XmlAttribute] public string id { get; set; }

        [XmlAttribute] public string ph { get; set; }

        [XmlAttribute] public string sa { get; set; }
    }
}
