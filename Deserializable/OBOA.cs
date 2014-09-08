using UnityEngine;
using System.Collections;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;

namespace Round2
{
    public class OBOA
    {
        [XmlArray("Objects")]
        public OBOAObject[] Objects;

        [XmlAttribute("id")]
        public int id;
    }
}
