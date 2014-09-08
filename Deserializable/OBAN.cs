using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace Round2
{
    public class _OBAN
    {
        [XmlElement("OBAN")]
        public OBAN @OBAN;
    }

    public class OBAN 
    {
        int _id;

        [XmlAttribute("id")]
        public int id
        {
            get
            {
                return _id;
            }
            set
            {
                Debug.LogWarning("[OBAN]:" + value);
                _id = value;
            }
        }

        public string Flags;
        public string InitialTransform;
        public string BaseTransform;
        public int FrameLength;
        public int FrameCount;
        public int HalfStopFrame;

        [XmlArray("KeyFrames")]
        public OBANKeyFrame[] KeyFrames;
    }
}
