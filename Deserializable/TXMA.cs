using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class _TXMA
    {
        [XmlElement("TXMA")]
        public TXMA @TXMA;
    }

    public class TXMA
    {
        [XmlAttribute]
        public int id;

        [XmlArray("Textures")]
        public Round2.Links.TXMPLNK[] Textures;
    }
}
