using UnityEngine;
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
        private _AISA m_characters;

        public _AISA Characters
        {
            get 
            {
                return m_characters;
            }
            set
            {
                m_characters = value;
            }
        }

        Dictionary<string, List<VRTX>> m_objectsTrs = new Dictionary<string, List<VRTX>>();

        void EmitVertex(string objhash, Vector3 pos, Vector2 uv, Int32 originalIdx)
        {
            if (!m_objectsTrs.ContainsKey(objhash))
            {
                m_objectsTrs.Add(objhash, new List<VRTX>());
            }

            m_objectsTrs[objhash].Add(new VRTX() { origIDX = originalIdx.Value, pos = new UnityEngine.Vector3(-pos.Value.x, pos.Value.y, pos.Value.z), uv = uv.Value, realIDX = m_objectsTrs[objhash].Count });
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
            for (int index = 0; index < Environment.AKEV.Quads.AGQG.Quads.Length; index++)
            {
                int txi = this.Environment.AKEV.QuadTextures.AGQR.Elements[index].Texture.Value;
                Links.TXMPLNK l_lnk = this.Environment.AKEV.Textures.TXMA.Textures[txi];
                AGQGQuad l_quad = Environment.AKEV.Quads.AGQG.Quads[index];

                if ((l_quad.Flags & GunkFlags.Ghost) != 0 || (l_quad.Flags & GunkFlags.StairsUp) != 0 || (l_quad.Flags & GunkFlags.StairsDown) != 0 || (l_quad.Flags & GunkFlags.DoorFrame) != 0 || (l_quad.Flags & GunkFlags.Furniture) != 0)
                {
                    continue;
                }

                string l_hash = new object[] 
                { 
                    l_quad.Flags, 
                    l_quad.ObjectId,  
                    (l_lnk != null && l_lnk.TXMP!= null ? l_lnk.TXMP.id : -1) 
                }.ArrayAsSml();
                
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
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[1].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[1].Value],
                    l_quad.Points[1]
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
                    this.Environment.AKEV.Points.PNTA.Positions[l_quad.Points[3].Value],
                    this.Environment.AKEV.TextureCoordinates.TXCA.TexCoords[l_quad.TextureCoordinates[3].Value],
                    l_quad.Points[3]
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
            }

            int l_counter = 0;

            foreach (string l_objname in m_objectsTrs.Keys)
            {
                GameObject g = new GameObject(l_objname);
                MeshFilter mf = g.AddComponent<MeshFilter>();
                object[] l_arr = l_objname.SmlToArray();
                l_counter = l_arr == null || l_arr[2] == null ? -1 : (int)l_arr[2];
                Material l_objmat = g.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));

                if (l_counter >= 0)
                {
                    Texture2DQuery.TexturePend(l_counter, tex => l_objmat.mainTexture = tex);
                }
                
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
            }
        }
    }
}
