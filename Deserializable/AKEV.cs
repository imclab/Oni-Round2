using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    [System.Flags]
    public enum GunkFlags
    {
        None = 0,
        DoorFrame = 1,
        Ghost = 2,
        StairsUp = 4,
        StairsDown = 8,
        Stairs = 16,
        Triangle = 64,
        Transparent = 128,
        TwoSided = 512,
        NoCollision = 2048,
        Invisible = 8192,
        NoObjectCollision = 16384,
        NoCharacterCollision = 32768,
        NoOcclusion = 65536,
        Danger = 131072,
        Horizontal = 524288,
        Vertical = 1048576,
        GridIgnore = 4194304,
        NoDecals = 8388608,
        Furniture = 16777216,
        ProjectionBit0 = 33554432,
        ProjectionBit1 = 67108864,
        SoundTransparent = 134217728,
        Impassable = 268435456,
    }

    public class _AKEV
    {
        [XmlElement("AKEV")]
        public AKEV @AKEV;
    }
    
    [XmlType("AKEV")]
    public class AKEV
    {
        [XmlAttribute("id")]
        public int id;

        public _PNTA Points;
        public _TXCA TextureCoordinates;

        _AGQG m_quads;

        public _AGQG Quads
        {
            get
            {
                return m_quads;
            }

            set
            {
                m_quads = value;
            }
        }

        private _AGQR m_textures;

        public _AGQR QuadTextures
        {
            get
            {
                return m_textures;
            }

            set
            {
                Debug.Log(value + "|" + value.AGQR.Elements.Length);
                m_textures = value;
            }
        }

        private _TXMA m_txma;
        
        public _TXMA Textures
        {
            get
            {
                return m_txma;
            }

            set
            {
                Debug.LogError(value.TXMA.Textures[0].TXMP.id);
                m_txma = value;
            }
        }
    }
}
