using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;

namespace Round2
{
    public class Vector2 : IXmlSerializable
    {
        public static UnityEngine.Vector2 FromString(string value)
        {
            float[] l_floats = new List<string>(value.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
            return new UnityEngine.Vector2(l_floats[0], l_floats[1]);
        }

        [XmlIgnore]
        string _value;

        UnityEngine.Vector2 value;

        [XmlIgnore]

        public UnityEngine.Vector2 Value
        {
            get
            {
                return value;
            }
        }

        public Vector2()
        {
        }

        public Vector2(string val)
        {
            _value = val;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            _value = reader.ReadString();
            value = FromString(_value);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(_value);
        }
    }
}
