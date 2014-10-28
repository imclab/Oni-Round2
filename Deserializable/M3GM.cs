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

        private Mesh m_bufferedMesh;

        public Mesh UnityMesh
        {
            get
            {
                if (m_bufferedMesh == null)
                {
                    int[] indiсes = this.TriangleStrips.IDXA.DecodeForM3GM_1();
                    UnityEngine.Vector3[] pts = new List<Round2.Vector3>(this.Points.PNTA.Positions).ConvertAll<UnityEngine.Vector3>(u => new UnityEngine.Vector3(-u.Value.x, u.Value.y, u.Value.z)).ToArray();
                    UnityEngine.Vector2[] uvs = new List<Round2.Vector2>(this.TextureCoordinates.TXCA.TexCoords).ConvertAll<UnityEngine.Vector2>(u => u.Value).ToArray();
                    UnityEngine.Vector3[] normals = new List<Round2.Vector3>(this.VertexNormals.VCRA.Normals).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
                    Mesh m = new Mesh();
                    m.vertices = pts;
                    m.uv = uvs;
                    m.normals = normals;
                    m.triangles = indiсes;
                    m.RecalculateBounds();
                    m_bufferedMesh = m;
                }

                return m_bufferedMesh;
            }
        }
    }
}
