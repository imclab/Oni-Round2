using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

namespace Round2
{
    public class _M3GA
    {
        [XmlElement("M3GA")]
        public M3GA @M3GA;
    }

    public class M3GA
    {
        [XmlAttribute(AttributeName = "id")]
        public int id
        {
            get;
            set;
        }

        public Links.M3GM._Link Geometries;
    }

    
}
