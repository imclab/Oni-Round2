using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _VCRA
    {
        public VCRA @VCRA;
    }

    public class VCRA
    {
        [XmlAttribute("id")]
        public int id;

        [XmlArray("Normals")]
        public Vector3[] Normals;
    }
}
