using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _AGQRElement
    {
        [XmlElement("AGQRElement")]
        public AGQRElement @AGQRElement;
    }

    [XmlType("Texture")]
    public class ACQGETexture : IXmlSerializable
    {
        [XmlIgnore]
        public int Value
        {
            get
            {
               return m_val;
            }
        }

        public ACQGETexture()
        {
        }

        public ACQGETexture(string val)
        {
            int.TryParse(val, out m_val);
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        int m_val;

        public void ReadXml(System.Xml.XmlReader reader)
        {
            string l_v = reader.ReadString();
            int.TryParse(l_v, out m_val);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(m_val.ToString());
        }
    }

    public class AGQRElement 
    {
        public ACQGETexture Texture;
    }
}
