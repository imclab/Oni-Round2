using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

namespace Round2
{
    public class _M3GM
    {
        public Round2.M3GM @M3GM;
    }

    public class M3GM
    {
        [XmlAttribute("id")]
        public int id;

        public _PNTA Points;
        public _VCRA VertexNormals;
        public _VCRA FaceNormals;
        public _TXCA TextureCoordinates;
        public _IDXA TriangleStrips;
        public _IDXA FaceNormalIndices;
        public _TXMP Texture; 
    }
}
