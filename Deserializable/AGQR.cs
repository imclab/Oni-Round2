using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _AGQR
    {
        [XmlElement("AGQR")]
        public AGQR @AGQR;
    }

    public class AGQR
    {
        [XmlArray("Elements")]
        public AGQRElement[] Elements;
    }
}


