﻿using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{   
    public class ONLV
    {
        [XmlAttribute("id")]
        public int id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        [XmlArray("Objects")]
        public OBOA[] Objects
        {
            get;
            set;
        }

        public _AKEV Environment;

        Dictionary<string, List<VRTX>> m_objectsTrs = new Dictionary<string, List<VRTX>>();

        void EmitVertex(string objhash, Vector3 pos, Vector2 uv, Int32 originalIdx)
        {
            if (!m_objectsTrs.ContainsKey(objhash))
            {
                m_objectsTrs.Add(objhash, new List<VRTX>());
            }

            m_objectsTrs[objhash].Add(new VRTX() { origIDX = originalIdx.Value, pos = pos.Value, uv = uv.Value, realIDX = m_objectsTrs[objhash].Count });
        }

        struct VRTX
        {
            public UnityEngine.Vector3 pos;
            public UnityEngine.Vector2 uv;
            public int origIDX;
            public int realIDX;
        }

        public void InitEnv()
        {
            //Debug.Log((GunkFlags)((GunkFlags.Ghost | GunkFlags.Danger | GunkFlags.DoorFrame) ^ (~(GunkFlags.Danger | GunkFlags.DoorFrame))));
            //return;
            //Debug.Log(Environment.AKEV.Quads.AGQG.Quads.Length + "|" + this.Environment.AKEV.QuadTextures.AGQR.Elements.Length);
            //foreach (AGQGQuad l_quad in Environment.AKEV.Quads.AGQG.Quads)
            for (int index = 0; index < Environment.AKEV.Quads.AGQG.Quads.Length; index++)
            {
                int txi = this.Environment.AKEV.QuadTextures.AGQR.Elements[index].Texture.Value;
                AGQGQuad l_quad = Environment.AKEV.Quads.AGQG.Quads[index];

                if ((l_quad.Flags & GunkFlags.Ghost) != 0 || (l_quad.Flags & GunkFlags.StairsUp) != 0 || (l_quad.Flags & GunkFlags.StairsDown) != 0 || (l_quad.Flags & GunkFlags.DoorFrame) != 0 || (l_quad.Flags & GunkFlags.Furniture) != 0)
                {
                    continue;
                }

                string l_hash = l_quad.Flags.ToString() + "|" + l_quad.ObjectId + " < tex : [" + txi + " ] >";

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[0].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[0].Value],
                    l_quad.Points[0]
                );

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[1].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[1].Value],
                    l_quad.Points[1]
                );

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[2].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[2].Value],
                    l_quad.Points[2]
                );

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[0].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[0].Value],
                    l_quad.Points[0]
                );

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[2].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[2].Value],
                    l_quad.Points[2]
                );

                EmitVertex
                (
                    l_hash,
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[3].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[3].Value],
                    l_quad.Points[3]
                );
            }

            int l_counter = 0;

            foreach (string l_objname in m_objectsTrs.Keys)
            {
                GameObject g = new GameObject(l_objname);
                MeshFilter mf = g.AddComponent<MeshFilter>();
                Material l_objmat = g.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                Texture2DQuery.TexturePend(this.Environment.AKEV.QuadTextures.AGQR.Elements[l_counter++].Texture.Value, tex => l_objmat.mainTexture = tex);
                Mesh m = new Mesh();
                List<UnityEngine.Vector3> vrts = new List<UnityEngine.Vector3>();
                List<UnityEngine.Vector2> uvs = new List<UnityEngine.Vector2>();
                List<int> idx = new List<int>();

                foreach (VRTX l_vrtx in m_objectsTrs[l_objname])
                {
                    vrts.Add(l_vrtx.pos);
                    idx.Add(l_vrtx.realIDX);
                    uvs.Add(l_vrtx.uv);
                }

                m.vertices = vrts.ToArray();
                m.triangles = idx.ToArray();
                m.uv = uvs.ToArray();
                m.RecalculateBounds();
                m.RecalculateNormals();
                mf.mesh = m;


                //break;
            }
        }
    }
}