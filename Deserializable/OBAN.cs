using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

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

        AnimationClip m_clip;
        
        public AnimationClip Clip(bool doorspecificFlip  = false)
        {
            if (m_clip == null)
            {
                List<Keyframe> rkFx = new List<Keyframe>();
                List<Keyframe> rkFy = new List<Keyframe>();
                List<Keyframe> rkFz = new List<Keyframe>();
                List<Keyframe> rkFw = new List<Keyframe>();
                List<Keyframe> kFx = new List<Keyframe>();
                List<Keyframe> kFy = new List<Keyframe>();
                List<Keyframe> kFz = new List<Keyframe>();

                foreach (OBANKeyFrame okf in KeyFrames)
                {
                    kFx.Add(new Keyframe(okf.RealTime, okf.Translation.Value.x));
                    kFy.Add(new Keyframe(okf.RealTime, okf.Translation.Value.y));
                    kFz.Add(new Keyframe(okf.RealTime, okf.Translation.Value.z));

                    rkFx.Add(new Keyframe(okf.RealTime, okf._Rotation.x));
                    rkFy.Add(new Keyframe(okf.RealTime, okf._Rotation.y));
                    rkFz.Add(new Keyframe(okf.RealTime, okf._Rotation.z));
                    rkFw.Add(new Keyframe(okf.RealTime, okf._Rotation.w));
                }

                m_clip = new AnimationClip();
                m_clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(kFx.ToArray()));

                m_clip.SetCurve("", typeof(Transform), "m_LocalPosition." + (doorspecificFlip ? "z" : "y"), new AnimationCurve(kFy.ToArray()));
                m_clip.SetCurve("", typeof(Transform), "m_LocalPosition." + (doorspecificFlip ? "y" : "z"), new AnimationCurve(kFz.ToArray()));
            }

            return m_clip;
        }
    }
}
