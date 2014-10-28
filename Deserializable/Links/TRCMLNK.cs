using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public partial class Links
    {
        [XmlType("Link")]
        public class TRCMLNK
        {
            public Round2.TRCM TRCM;
        }
    }
}
