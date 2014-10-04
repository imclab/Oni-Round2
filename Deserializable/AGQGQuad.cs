using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class AGQGQuad
    {
        [XmlArray("Points")]
        public Int32[] Points;

        [XmlArray("TextureCoordinates")]
        public Int32[] TextureCoordinates;
        public int ObjectId;

        public GunkFlags Flags;
    }
}