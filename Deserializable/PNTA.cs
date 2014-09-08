using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _PNTA
    {
        public PNTA @PNTA;
    }

    public class PNTA
    {
        [XmlAttribute("id")]
        public int id;

        [XmlElement("BoundingBox")]
        public Round2.BoundingBox @BoundingBox;
        [XmlElement("BoundingSphere")]
        public Round2.BoundingSphere @BoundingSphere;

        [XmlArray("Positions")]
        public Vector3[] Positions;
    }
}
