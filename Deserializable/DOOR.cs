using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class DOOR
    {
        public static Dictionary<string, DOOR> m_doorClasses = new Dictionary<string, DOOR>();

        [XmlAttribute("id")]
        public int id;
        [XmlElement("Animation")]
        public _OBAN Animation;
    }
}
