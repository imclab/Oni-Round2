using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class TRCM 
    {
        [XmlAttribute]
        public int id;
        public int BodyPartCount;
        public _TRGA Geometry;
        public _TRTA Position;
        public _TRIA Hierarchy;
    }
}
