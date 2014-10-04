using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class OBOAObject
    {
        public OBOAObject()
        {
            m_go = new GameObject();
            m_go.AddComponent<OBOA_Instantiator>().m_obj = this;
        }

        [XmlIgnore]
        _M3GA m_m3ga;

        public _M3GA Geometry
        {
            get
            {
                return m_m3ga;
            }

            set
            {
                m_m3ga = value;
            }
        }

        [XmlIgnore]
        GameObject m_go;
        public _OBAN Animation;
        public ENVP Particle;
        public string Flags;
        public int DoorGunkId;
        public int DoorId;
        [XmlIgnore]
        private string m_pht;

        [XmlElement(elementName : "PhysicsType")]
        public string PhysicsType
        {
            private set
            {
                m_pht = value;
            }

            get
            {
                return m_pht;
            }
        }

        public int ScriptId;
        public string Position
        {
            set
            {
                //float[] l_floats = new List<string>(value.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                m_go.transform.position = Vector3.FromString(value); //new UnityEngine.Vector3(l_floats[0], l_floats[1], l_floats[2]);
            }
            get
            {
                return m_go.transform.position.ToString();
            }
        }

        //TODO: refactor to proper way : must deserialize strings only once per object, not once per call
        [XmlIgnore]
        public UnityEngine.Vector3 _Position
        {
            get
            {
                return m_go.transform.position;
            }
        }

        public string Rotation
        {
            set
            {
                float[] l_floats = new List<string>(value.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                Oni.Quaternion _q;
                
                if (l_floats.Length > 3)
                {
                    _q = new Oni.Quaternion(l_floats[0], l_floats[1], l_floats[2], l_floats[3]);
                }
                else
                {
                    _q = Oni.Quaternion.CreateFromEulerXYZ(l_floats[0], l_floats[1], l_floats[2]);
                }
                Oni.Vector3 euls =  _q.ToEulerXYZ();

                m_go.transform.rotation = new UnityEngine.Quaternion(_q.X, _q.Y, _q.Z, _q.W);
            }
            get
            {
                return m_go.transform.rotation.ToString();
            }
        }

        [XmlIgnore]
        public UnityEngine.Quaternion _Rotation
        {
            get
            {
                return m_go.transform.rotation;
            }
        }

        public float Scale
        {
            get
            {
                return m_go.transform.localScale.magnitude;
            }
            set
            {
                m_go.transform.localScale = new UnityEngine.Vector3(value, value, value);
            }
        }
        public string Transform;

        public string Name
        {
            set
            {
                m_go.name = value;
            }
            get
            {
                return m_go.name;
            }
        }
    }
}
