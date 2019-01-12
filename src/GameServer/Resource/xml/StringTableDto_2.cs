using System.Xml.Serialization;

namespace NeoNetsphere.Resource.xml
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "string_table")]
    public class StringTableDto_2
    {
        [XmlElement("string")] public StringTableStringDto_2[] @string { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class StringTableStringDto_2
    {
        [XmlAttribute] public string key { get; set; }

        [XmlAttribute] public string kor { get; set; }

        [XmlAttribute] public string ger { get; set; }

        [XmlAttribute] public string eng { get; set; }

        [XmlAttribute] public string tur { get; set; }

        [XmlAttribute] public string fre { get; set; }

        [XmlAttribute] public string spa { get; set; }

        [XmlAttribute] public string ita { get; set; }

        [XmlAttribute] public string rus { get; set; }

        [XmlAttribute] public string ame { get; set; }

        [XmlAttribute] public string cns { get; set; }

        [XmlAttribute] public string tha { get; set; }

        [XmlAttribute] public string twn { get; set; }

        [XmlAttribute] public string jap { get; set; }

        [XmlAttribute] public string idn { get; set; }

        public override string ToString()
        {
            return key;
        }
    }
}
