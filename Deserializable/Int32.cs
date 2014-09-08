using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;

namespace Round2
{
    public class Int32 : IXmlSerializable
    {
        public static System.Int32 FromString(string value)
        {
            return System.Int32.Parse(value);
        }

        [XmlIgnore]
        string _value;

        System.Int32 value;

        [XmlIgnore]
        public System.Int32 Value
        {
            get
            {
                return value;
            }
        }

        public Int32()
        {
        }

        public Int32(string val)
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
