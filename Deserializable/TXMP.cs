using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public class _TXMP
    {
        public TXMP @TXMP;
    }

    /// <summary>
    /// Does not uses animation, data offset or envmap. See comments section to discover more
    /// </summary>
    public class TXMP
    {
        int m_id;

        [XmlAttribute("id")]
        public int id
        {
            get 
            {
                return m_id;
            }

            set
            {
                m_id = value;
                Texture2DQuery.TexturePend(m_id, u => m_tex = u);
            }
        }

        public string Flags;
        public int Width;
        public int Height;
        public string Format;

        UnityEngine.Texture2D m_tex = null;

        [XmlIgnore]
        public Texture2D UnityTexture
        {
            get
            {
                return NewBehaviourScript.m_textureCache.ContainsKey(id) ? NewBehaviourScript.m_textureCache[id] : m_tex;
            }
        }


        /*<Flags>HasMipMaps SwapBytes</Flags>
                    <Width>64</Width>
                    <Height>64</Height>
                    <Format>DXT1</Format>
                    <Animation></Animation>
                    <EnvMap></EnvMap>
                    <DataOffset>79520</DataOffset>*/
    }
}
