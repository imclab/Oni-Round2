using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Round2
{
   
    public class _ONCC
    {
        [XmlElement("ONCC")]
        public ONCC @ONCC;
    }

    public class ONCC
    {
        public enum Bodyparts
        {
            /*
            0	00	pelvis/none
            01	01	left thigh
            02	02	left calf
            03	03	left foot
            04	04	right thigh
            05	05	right calf
            06	06	right foot
            07	07	mid
            08	08	chest
            09	09	neck
            0A	10	head
            0B	11	left shoulder
            0C	12	left arm
            0D	13	left wrist
            0E	14	left fist
            0F	15	right shoulder
            10	16	right arm
            11	17	right wrist
            12	18	right fist
             */
            pelvis = 0,
            left_thigh = 0x1,
            left_calf = 0x2,
            left_foot = 0x3,
            right_thigh = 0x4,
            right_calf = 0x5,
            right_foot = 0x6,
            mid = 0x7,
            chest = 0x8,
            neck = 0x9,
            head = 0xA,
            left_shoulder = 0xB,
            left_arm = 0xC,
            left_wrist = 0xD,
            left_fist = 0xE,
            right_shoulder = 0xF,
            right_arm = 0x10,
            right_wrist = 0x11,
            right_fist = 0x12
        }


        [XmlAttribute]
        public int id;

        _TRBS m_bodySet;

        public _TRBS BodySet
        {
            get
            {
                return m_bodySet;
            }

            set
            {
                m_bodySet = value;
            }
        }

        public _TRMA BodyTextures;
    }
}
