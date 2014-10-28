using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TRBS
    {
        public TRBS @TRBS;
    }

    public class TRBS
    {
        [XmlAttribute]
        public int id;

        [XmlArray("Elements")]
        public Links.TRCMLNK[] Elements;
    }
}
