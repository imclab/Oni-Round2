using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    [XmlType("DOOR")]
    public class BINADOOR
    {
        [XmlAttribute("Id")]
        public int Id;

        [XmlElement("Header")]
        public HEADER Header;

        [XmlElement("OSD")]
        public OSD _OSD;

        [XmlType("OSD")]
        public class OSD
        {
            public string Class;
            public int DoorId;
            [XmlElement("SquaredActivationRadius")]
            public float SquaredActivationRadius;
        }

        [XmlType("Header")]
        public class HEADER
        {
            public string Flags;
            public Vector3 Position;
            public Vector3 Rotation;
        }
    }

    
}
