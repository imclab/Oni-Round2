using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TRMA
    {
        public TRMA @TRMA;
    }

    public class TRMA
    {
        [XmlAttribute]
        public int id;

        [XmlArray("Textures")]
        public Links.TXMPLNK[] Textures;
    }
}
