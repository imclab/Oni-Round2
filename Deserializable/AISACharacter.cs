using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class AISACharacter 
    {
        static Dictionary<string, List<AISACharacter>> m_teams = new Dictionary<string, List<AISACharacter>>();

        public static AISACharacter[] GetByName(string teamName)
        {
            if (m_teams.ContainsKey(teamName))
            {
                return m_teams[teamName].ToArray();
            }

            return new AISACharacter[] { };
        }

        public string Name;
        public int ScriptId;
        public int FlagId;
        string m_team;

        public string Team
        {
            get
            {
                return m_team;
            }

            set
            {
                if (!m_teams.ContainsKey(value))
                {
                    m_teams.Add(value, new List<AISACharacter>());
                }

                m_teams[value].Add(this);
                m_team = value;
            }
        }

        public _ONCC Class;
        public int Ammo;

        public void TMP_InstallNewCharacter()
        {
            List<Mesh> l_meshes = new List<Mesh>();
            List<GameObject> l_parts = new List<GameObject>();

            foreach (Links.M3GMLNK geomery in this.Class.ONCC.BodySet.TRBS.Elements[4].TRCM.Geometry.TRGA.Geometries)
            {
                l_meshes.Add(geomery.M3GM.UnityMesh);
            }

            for (int i = 0; i < this.Class.ONCC.BodySet.TRBS.Elements[4].TRCM.BodyPartCount; i++)
            {
                GameObject l_newest;
                l_parts.Add(l_newest = GameObject.CreatePrimitive(PrimitiveType.Cube));
                l_newest.name = ((ONCC.Bodyparts)i).ToString();
                l_newest.GetComponent<MeshFilter>().mesh = l_meshes[i];
                GameObject.Destroy(l_newest.collider);
                l_newest.renderer.material.shader = Shader.Find("TwoSidedDiffuse");
            }

            for (int i = 0; i < this.Class.ONCC.BodySet.TRBS.Elements[4].TRCM.BodyPartCount; i++)
            {
                l_parts[i].transform.parent = l_parts[this.Class.ONCC.BodySet.TRBS.Elements[4].TRCM.Hierarchy.TRIA.Elements[i].Parent].transform;
                Vector3 lpos =  this.Class.ONCC.BodySet.TRBS.Elements[4].TRCM.Position.TRTA.Translations[i];
                l_parts[i].transform.localPosition = new UnityEngine.Vector3(-lpos.Value.x, lpos.Value.y, lpos.Value.z);
                Material l_m = l_parts[i].renderer.material;
                Texture2DQuery.TexturePend(this.Class.ONCC.BodyTextures.TRMA.Textures[i].TXMP.id, u => l_m.mainTexture = u);
            }
        }
    }
}
