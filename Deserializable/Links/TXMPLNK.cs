using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

namespace Round2
{
    public partial class Links
    {
        [XmlType("Link")]
        public class TXMPLNK
        {
            public Round2.TXMP TXMP;
        }
    }
}
