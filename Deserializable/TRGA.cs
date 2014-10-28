using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TRGA
    {
        public TRGA @TRGA;
    }

    public class TRGA
    {
        [XmlAttribute]
        public int id;

        public Links.M3GMLNK[] Geometries;
    }
}
