using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
    public class _IDXA
    {
        public IDXA @IDXA;
    }

    public class IDXA
    {
        //public const int IDXA_HBIT = -2147483648;
        [XmlAttribute("id")]
        public int id;

        /// <summary>
        /// IDXA explanation for some cases : http://wiki.oni2.net/OBD:IDXA_M3GM_1
        /// Use decoding for triangle strips
        /// Use raw values for face normal extraction
        /// </summary>
        [XmlArray("Indices")]
        public Int32[] Indices;

        /// <summary>
        /// Fix triangle order using decoding table for M3GM 1 case
        /// </summary>
        /// <returns></returns>
        public int[] DecodeForM3GM_1()
        {
            List<int> l_idxlst = new List<int>();
            List<int> l_tmpidxlst = new List<int>();
            int[][] l_stripSchemes = new int[][] 
            {
                new int[] { 0, 1, 2 },
                new int[] { 2, 1, 3 },
            };

            for (int i = 0; i < Indices.Length; i++ )
            {
                if (Indices[i].Value < 0)
                {
                    l_tmpidxlst.Clear();
                }

                l_tmpidxlst.Add(Indices[i + 0].Value << 1 >> 1);

                switch (l_tmpidxlst.Count)
                { 
                    case 3:
                        l_idxlst.Add(l_tmpidxlst[0]);
                        l_idxlst.Add(l_tmpidxlst[1]);
                        l_idxlst.Add(l_tmpidxlst[2]);
                        break;

                    case 4:
                        l_idxlst.Add(l_tmpidxlst[2]);
                        l_idxlst.Add(l_tmpidxlst[1]);
                        l_idxlst.Add(l_tmpidxlst[3]);
                        l_tmpidxlst.RemoveRange(0, 2);
                        break;
                }
            }

            return l_idxlst.ToArray();
        }
    }
}
