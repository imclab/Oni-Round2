using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class _AISA
    {
        [XmlElement("AISA")]
        public AISA @AISA;
    }

    public class AISA
    {
        [XmlAttribute]
        public int id;

        [XmlArray("Characters")]
        public AISACharacter[] Characters;
    }
}
