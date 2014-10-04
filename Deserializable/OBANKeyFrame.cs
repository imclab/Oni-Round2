using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class OBANKeyFrame
    {
        public string Rotation;

        [XmlIgnore]
        public UnityEngine.Quaternion _Rotation
        {
            get
            {
                float[] l_floats = new List<string>(Rotation.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                Oni.Quaternion _q;

                if (l_floats.Length > 3)
                {
                    _q = new Oni.Quaternion(l_floats[0], l_floats[1], l_floats[2], l_floats[3]);
                }
                else
                {
                    _q = Oni.Quaternion.CreateFromEulerXYZ(l_floats[0], l_floats[1], l_floats[2]);
                }

                Oni.Vector3 euls = _q.ToEulerXYZ();

                return new UnityEngine.Quaternion(_q.X, _q.Y, _q.Z, _q.W);
            }
        }

        public Vector3 Translation;
        public int Time;

        [XmlIgnore]
        public float RealTime
        {
            get
            {
                return Time * (1 / 60f);
            }
        }
    }
}