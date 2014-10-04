using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _AGQG
    {
        public AGQG @AGQG; 
    }

    public class AGQG
    {
        [XmlAttribute("id")]
        public int id;

        [XmlArray("Quads")]
        public AGQGQuad[] Quads;
    }
}
