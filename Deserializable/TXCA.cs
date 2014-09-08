using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TXCA
    {
        public TXCA @TXCA;
    }

    public class TXCA
    {
        [XmlAttribute("id")]
        public int id;

        [XmlArray("TexCoords")]
        public Round2.Vector2[] TexCoords;
    }
}
