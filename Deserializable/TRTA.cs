using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TRTA
    {
        public TRTA @TRTA;
    }

    public class TRTA 
    {
        [XmlAttribute]
        public int id;

        public Round2.Vector3[] Translations;
    }
}
