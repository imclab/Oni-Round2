using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TRIA
    {
        public TRIA @TRIA;
    }

    public class TRIA
    {
        [XmlAttribute]
        public int id;

        [XmlArray("Elements")]
        public TRIAElement[] Elements;
    }
}
