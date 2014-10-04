using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIANIMCONTROL : MonoBehaviour 
{
    [System.Flags]
    enum AnimFlags
    {
        jump =          1 << 0,
        jump_fw =       1 << 1,
        jump_bk =       1 << 2,
        jump_rt =       1 << 3,
        jump_lt =       1 << 4,
        crouch =        1 << 5,
        idle =          1 << 6,
        land =          1 << 7,
        run =           1 << 8,
        run_bk =        1 << 9,
        ss_lt =         1 << 10,
        ss_rt =         1 << 11,
        stepa =         1 << 12,
        stepb =         1 << 13,
        start =         1 << 14,
        stop =          1 << 15,
        lt =            1 << 16,
        rt =            1 << 17,
        idle1 =         1 << 18,
        ss_lt_run =     1 << 19,//needed for transition from ss_lt to run
        ss_rt_run =     1 << 20,//needed for transition from ss_rt to run
        crouch2idlea =  1 << 21,
        crouch2idleb =  1 << 22,
        idle2croucha =  1 << 23,
        idle2crouchb =  1 << 24,
        fw =            1 << 25,
        bk =            1 << 26,
    }

    float jumpHeight = 25;
    AnimFlags m_lastFlags = AnimFlags.idle1;
    AnimFlags m_prevFlags = 0;

    bool m_lockCursor = false;

    string GetAnim()
    {
        m_prevFlags = m_lastFlags;
        m_targetAngle = Quaternion.Euler(0, 0, 0);

        switch (m_lastFlags)
        {
            case AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.rt:
            case AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.lt:

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.start;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.W))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.start;
                        }
                        else
                        {
                            m_lastFlags = AnimFlags.crouch2idleb;
                        }
                    }
                }
                else
                {
                    if (!Input.GetKey(KeyCode.S))
                    {
                        if (Input.GetKey(KeyCode.A))
                        {
                            m_targetAngle = Quaternion.Euler(0, 90, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, -90, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.W))
                        {
                            m_lastFlags = AnimFlags.crouch | AnimFlags.run | AnimFlags.lt;
                        }
                        else
                        {
                            m_lastFlags = AnimFlags.crouch | AnimFlags.idle;
                        }
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.A))
                        {
                            m_targetAngle = Quaternion.Euler(0, 45, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, -45, 0);
                            break;
                        }
                    }
                }

                break;
            case AnimFlags.crouch | AnimFlags.run | AnimFlags.lt:
            case AnimFlags.crouch | AnimFlags.run | AnimFlags.rt:

                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.run | AnimFlags.start;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.S))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.start;
                        }
                        else
                        {
                            m_lastFlags = AnimFlags.crouch2idleb;
                        }
                    }
                }
                else
                {
                    if (!Input.GetKey(KeyCode.W))
                    {
                        if (Input.GetKey(KeyCode.A))
                        {
                            m_targetAngle = Quaternion.Euler(0, -90, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, 90, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.S))
                        {
                            m_lastFlags = AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.lt;
                        }
                        else
                        {
                            m_lastFlags = AnimFlags.crouch | AnimFlags.idle;
                        }
                    }

                    else
                    {
                        if (Input.GetKey(KeyCode.A))
                        {
                            m_targetAngle = Quaternion.Euler(0, -45, 0);
                            break;
                        }

                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, 45, 0);
                            break;
                        }
                    }
                }
                break;
            case AnimFlags.crouch | AnimFlags.idle:
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    m_lastFlags = AnimFlags.crouch2idleb;   
                }

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    m_lastFlags = AnimFlags.crouch | AnimFlags.run | AnimFlags.lt;
                }

                if(Input.GetKey(KeyCode.S))
                {
                     m_lastFlags = AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.lt;
                }

                break;
            //end of crouch block
            case AnimFlags.idle1:
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.run | AnimFlags.stepa;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.stepa;
                        break;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        m_lastFlags = AnimFlags.ss_lt | AnimFlags.stepa;

                        break;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.ss_rt | AnimFlags.stepa;
                        break;
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        m_lastFlags = AnimFlags.idle2croucha;
                    }
                }
                break;
            case AnimFlags.run | AnimFlags.lt:
                {
                    if (!Input.GetKey(KeyCode.W))
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.ss_lt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.A))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.ss_rt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.S))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.rt;
                            break;
                        }

                        m_lastFlags = AnimFlags.run | AnimFlags.stop;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, 45, 0);
                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.A))
                            {
                                m_targetAngle = Quaternion.Euler(0, -45, 0);
                            }
                        }
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_fw | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                } 
                break;
            case AnimFlags.run | AnimFlags.rt:
                {
                    if (!Input.GetKey(KeyCode.W))
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.ss_lt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.A))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.ss_rt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.S))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.rt;
                            break;
                        }

                        m_lastFlags = AnimFlags.run | AnimFlags.stop;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, 45, 0);
                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.A))
                            {
                                m_targetAngle = Quaternion.Euler(0, -45, 0);
                            }
                        }
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_fw | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            case AnimFlags.run | AnimFlags.start:
            case AnimFlags.run | AnimFlags.stepa:

                if (Input.GetKey(KeyCode.Space))
                {
                    m_lastFlags = AnimFlags.jump_fw | AnimFlags.start;
                    m_upperVector = m_jumpVal;
                    m_jumpStart = Time.time;
                    m_rememberedSpeed = m_motionVector;
                    break;
                }

                break;
            //end of forward
            case AnimFlags.run_bk | AnimFlags.lt:
                {
                    if (!Input.GetKey(KeyCode.S))
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.ss_lt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.A))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.ss_rt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.W))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.rt;
                            break;
                        }

                        m_lastFlags = AnimFlags.run_bk | AnimFlags.stop;
                        break;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, -45, 0);
                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.A))
                            {
                                m_targetAngle = Quaternion.Euler(0, 45, 0);
                            }
                        }
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_bk | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            case AnimFlags.run_bk | AnimFlags.rt:
                {
                    if (!Input.GetKey(KeyCode.S))
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.ss_lt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.A))
                        {
                            m_lastFlags = AnimFlags.run_bk | AnimFlags.ss_rt;
                            break;
                        }

                        if (Input.GetKey(KeyCode.W))
                        {
                            m_lastFlags = AnimFlags.run | AnimFlags.rt;
                            break;
                        }

                        m_lastFlags = AnimFlags.run_bk | AnimFlags.stop;
                        break;
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.D))
                        {
                            m_targetAngle = Quaternion.Euler(0, -45, 0);
                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.A))
                            {
                                m_targetAngle = Quaternion.Euler(0, 45, 0);
                            }
                        }
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_bk | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;

            case AnimFlags.run_bk | AnimFlags.start:
            case AnimFlags.run_bk | AnimFlags.stepa:

                if (Input.GetKey(KeyCode.Space))
                {
                    m_lastFlags = AnimFlags.jump_bk | AnimFlags.start;
                    m_upperVector = m_jumpVal;
                    m_jumpStart = Time.time;
                    m_rememberedSpeed = m_motionVector;
                    break;
                }

                break;
            //end of backward
            case AnimFlags.ss_lt | AnimFlags.lt:
                {
                    if (!Input.GetKey(KeyCode.D))
                    {   
                        m_lastFlags = AnimFlags.ss_lt | AnimFlags.stop;
                    }

                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.ss_lt_run;
                    }

                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.lt;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.ss_rt | AnimFlags.stepa;
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_lt | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            case AnimFlags.ss_lt | AnimFlags.rt:
                {
                    if (!Input.GetKey(KeyCode.D))
                    {
                        m_lastFlags = AnimFlags.ss_lt | AnimFlags.stop;
                    }

                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.ss_lt_run;
                    }

                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.lt;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.ss_rt | AnimFlags.stepa;
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_lt | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            //end of ss_lt
            case AnimFlags.ss_rt | AnimFlags.lt:
                {
                    if (!Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.ss_rt | AnimFlags.stop;
                    }

                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.ss_rt_run;
                    }

                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.lt;
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        m_lastFlags = AnimFlags.ss_lt | AnimFlags.stepa;
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_rt | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            case AnimFlags.ss_rt | AnimFlags.rt:
                {
                    if (!Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.ss_rt | AnimFlags.stop;
                    }

                    if (Input.GetKey(KeyCode.W))
                    {
                        m_lastFlags = AnimFlags.ss_rt_run;
                    }

                    if (Input.GetKey(KeyCode.S))
                    {
                        m_lastFlags = AnimFlags.run_bk | AnimFlags.lt;
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        m_lastFlags = AnimFlags.ss_lt | AnimFlags.stepa;
                    }

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_lastFlags = AnimFlags.jump_rt | AnimFlags.start;
                        m_upperVector = m_jumpVal;
                        m_jumpStart = Time.time;
                        m_rememberedSpeed = m_motionVector;
                        break;
                    }
                }
                break;
            //end of transition from run to ss_rt
        }

        string animname = "";

        foreach (string s in m_lastFlags.ToString().Split(",".ToCharArray()))
        {
            switch (s.Replace(" ",""))
            {
                case "ss_rt":
                    animname += "ss_rt";
                    break;
                case "ss_lt":
                    animname += "ss_lt";
                    break;
                case "run_bk":
                    animname += "run_bk";
                    break;
                case "stepa":
                    animname +=  "1stepa";
                    break;
                case "stepb":
                    animname += "1stepb";
                    break;
                default:
                    animname += s.Replace(" ", "");
                    break;
                case "rt":
                    animname += "rt";
                    break;
                case "lt":
                    animname += "lt";
                    break;
            }

            animname += "_";
        }

        animname = animname.Remove(animname.Length - 1);

        if ((m_lastFlags & AnimFlags.run) != 0)
        {
            switch (animname)
            {
                case "run_1stepa":
                    animname = "run1stepa";
                    break;
                case "run_1stepb":
                    animname = "run1stepb";
                    break;
                case "run_start":
                    animname = "runstart";
                    break;
                case "run_stop":
                    animname = "runstop";
                    break;
            }
        }
    
        return animname;
    }

    public float m_jumpVal = 45;

    void OnLand()
    {
        switch (m_lastFlags)
        {
            case AnimFlags.jump_lt | AnimFlags.start:
            case AnimFlags.jump_lt | AnimFlags.idle:
                m_lastFlags = AnimFlags.jump_lt | AnimFlags.land;

                if (Input.GetKey(KeyCode.D))
                {
                    m_lastFlags |= AnimFlags.lt;
                    break;
                }
                break;
            //end of rt_jump

            case AnimFlags.jump_rt | AnimFlags.start:
            case AnimFlags.jump_rt | AnimFlags.idle:
                m_lastFlags = AnimFlags.jump_rt | AnimFlags.land;

                if (Input.GetKey(KeyCode.A))
                {
                    m_lastFlags |= AnimFlags.rt;
                    break;
                }
                break;
            //end of rt_jump

            case AnimFlags.jump_bk | AnimFlags.start:
            case AnimFlags.jump_bk | AnimFlags.idle:
                m_lastFlags = AnimFlags.jump_bk | AnimFlags.land;

                if (Input.GetKey(KeyCode.S))
                {
                    m_lastFlags |= AnimFlags.bk;
                    break;
                }
                break;
            //end of backward jump

            case AnimFlags.jump_fw | AnimFlags.start:
            case AnimFlags.jump_fw | AnimFlags.idle:
                m_lastFlags = AnimFlags.jump_fw | AnimFlags.land;

                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags |= AnimFlags.fw;
                    break;
                }
                break;
            //end of forward + jump
            case AnimFlags.jump | AnimFlags.start:
            case AnimFlags.jump | AnimFlags.idle:

                m_lastFlags = AnimFlags.jump | AnimFlags.land;

                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags |= AnimFlags.fw;
                    break;
                }
                break;
        }

        m_upperVector = 0;
        m_jumpStart = 0;
        m_waitingForLanding = false;
        m_rememberedSpeed = Vector3.zero;
    }

    void OnLastFrame()
    {
        switch (m_lastFlags)
        {
            case AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.rt:
                m_lastFlags = AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.lt;
                break;

            case AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.lt:
                m_lastFlags = AnimFlags.crouch | AnimFlags.run_bk | AnimFlags.rt;
                break;

            case AnimFlags.crouch | AnimFlags.run | AnimFlags.rt:
                m_lastFlags = AnimFlags.crouch | AnimFlags.run | AnimFlags.lt;
                break;

            case AnimFlags.crouch | AnimFlags.run | AnimFlags.lt:
                m_lastFlags = AnimFlags.crouch | AnimFlags.run | AnimFlags.rt;
                break;

            case AnimFlags.idle2croucha:
                m_lastFlags = AnimFlags.idle2crouchb;
                break;

            case AnimFlags.idle2crouchb:

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    m_lastFlags = AnimFlags.crouch | AnimFlags.idle;
                }
                else
                {
                    m_lastFlags = AnimFlags.crouch2idleb;
                }

                break;

            case AnimFlags.crouch2idleb:
                m_lastFlags = AnimFlags.crouch2idlea;
                break;

            case AnimFlags.crouch2idlea:
                m_lastFlags = AnimFlags.idle1;
                break;

            //end of crouching block
            case AnimFlags.jump_lt | AnimFlags.land:
            case AnimFlags.jump_lt | AnimFlags.land | AnimFlags.lt:
                if (Input.GetKey(KeyCode.D))
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.rt;
                }
                else
                {
                    m_lastFlags = AnimFlags.idle1;
                }

                break;

            case AnimFlags.jump_lt | AnimFlags.start:
                m_lastFlags = AnimFlags.jump_lt | AnimFlags.idle;
                m_waitingForLanding = true;
                break;

            //end of jump lt
            case AnimFlags.jump_rt | AnimFlags.land:
            case AnimFlags.jump_rt | AnimFlags.land | AnimFlags.rt:
                if (Input.GetKey(KeyCode.A))
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.lt;
                }
                else
                {
                    m_lastFlags = AnimFlags.idle1;
                }

                break;

            case AnimFlags.jump_rt | AnimFlags.start:
                m_lastFlags = AnimFlags.jump_rt | AnimFlags.idle;
                m_waitingForLanding = true;
                break;

            //end of jump rt

            case AnimFlags.jump_bk | AnimFlags.land:
            case AnimFlags.jump_bk | AnimFlags.land | AnimFlags.bk:
                if (Input.GetKey(KeyCode.S))
                {
                    m_lastFlags = AnimFlags.run_bk | AnimFlags.rt;
                }
                else
                {
                    m_lastFlags = AnimFlags.idle1;
                }

                break;

            case AnimFlags.jump_bk | AnimFlags.start:
                m_lastFlags = AnimFlags.jump_bk | AnimFlags.idle;
                m_waitingForLanding = true;
                break;
            //end of jump_bk

            case AnimFlags.jump_fw | AnimFlags.land:
            case AnimFlags.jump_fw | AnimFlags.land | AnimFlags.fw:
                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.lt;
                }
                else
                {
                    m_lastFlags = AnimFlags.idle1;
                }
                break;
            case AnimFlags.jump_fw | AnimFlags.start:
                m_lastFlags = AnimFlags.jump_fw | AnimFlags.idle;
                m_waitingForLanding = true;
                break;
            //end of jump fw
            case AnimFlags.jump | AnimFlags.land | AnimFlags.fw:
                m_lastFlags = AnimFlags.run | AnimFlags.lt;
                break;
            case AnimFlags.jump | AnimFlags.land:
                m_lastFlags = AnimFlags.idle1;
                break;
            case AnimFlags.jump | AnimFlags.start:
                m_lastFlags = AnimFlags.jump | AnimFlags.idle;
                m_waitingForLanding = true;
                break;
            //end of jump idle
            case AnimFlags.idle1:
                //do nothing?
                break;
            case AnimFlags.run | AnimFlags.stepa:
                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.start;
                }
                else
                {
                    if (Input.GetKey(KeyCode.D))
                    {
                        m_lastFlags = AnimFlags.run | AnimFlags.ss_lt;

                        break;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        m_lastFlags = AnimFlags.run | AnimFlags.ss_rt;
                        break;
                    }

                    m_lastFlags = AnimFlags.run | AnimFlags.stepb;
                }

                
                break;
            case AnimFlags.run | AnimFlags.stepb:
                m_lastFlags = AnimFlags.idle1;
                break;
            case AnimFlags.run | AnimFlags.start:
                m_lastFlags = AnimFlags.run | AnimFlags.rt;
                break;
            case AnimFlags.run | AnimFlags.rt:
                m_lastFlags = AnimFlags.run | AnimFlags.lt;
                break;
            case AnimFlags.run | AnimFlags.lt:
                m_lastFlags = AnimFlags.run | AnimFlags.rt;
                break;
            case AnimFlags.run | AnimFlags.stop:
                m_lastFlags = AnimFlags.idle1;
                break;

            //end of forward

            case AnimFlags.run_bk | AnimFlags.stepa:
                if (Input.GetKey(KeyCode.S))
                {
                    m_lastFlags = AnimFlags.run_bk | AnimFlags.start;
                }
                else
                {
                    m_lastFlags = AnimFlags.run_bk | AnimFlags.stepb;
                }
                break;
            case AnimFlags.run_bk | AnimFlags.stepb:
                m_lastFlags = AnimFlags.idle1;
                break;
            case AnimFlags.run_bk | AnimFlags.start:
                m_lastFlags = AnimFlags.run_bk | AnimFlags.rt;
                break;
            case AnimFlags.run_bk | AnimFlags.rt:
                m_lastFlags = AnimFlags.run_bk | AnimFlags.lt;
                break;
            case AnimFlags.run_bk | AnimFlags.lt:
                m_lastFlags = AnimFlags.run_bk | AnimFlags.rt;
                break;
            case AnimFlags.run_bk | AnimFlags.stop:
                m_lastFlags = AnimFlags.idle1;
                break;
            //end of backward
            case AnimFlags.ss_lt | AnimFlags.stepa:
                if (Input.GetKey(KeyCode.D))
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.start;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.stepb;
                }
                break;
            case AnimFlags.ss_lt | AnimFlags.stepb:
                m_lastFlags = AnimFlags.idle1;
                break;
            case AnimFlags.ss_lt | AnimFlags.start:
                m_lastFlags = AnimFlags.ss_lt | AnimFlags.lt;
                break;
            case AnimFlags.ss_lt | AnimFlags.rt:
                m_lastFlags = AnimFlags.ss_lt | AnimFlags.lt;
                break;
            case AnimFlags.ss_lt | AnimFlags.lt:
                m_lastFlags = AnimFlags.ss_lt | AnimFlags.rt;
                break;
            case AnimFlags.ss_lt | AnimFlags.stop:
                m_lastFlags = AnimFlags.idle1;
                break;
            //end of ss_lt
            case AnimFlags.ss_rt | AnimFlags.stepa:
                if (Input.GetKey(KeyCode.A))
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.start;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.stepb;
                }
                break;
            case AnimFlags.ss_rt | AnimFlags.stepb:
                m_lastFlags = AnimFlags.idle1;
                break;
            case AnimFlags.ss_rt | AnimFlags.start:
                m_lastFlags = AnimFlags.ss_rt | AnimFlags.rt;
                break;
            case AnimFlags.ss_rt | AnimFlags.rt:
                m_lastFlags = AnimFlags.ss_rt | AnimFlags.lt;
                break;
            case AnimFlags.ss_rt | AnimFlags.lt:
                m_lastFlags = AnimFlags.ss_rt | AnimFlags.rt;
                break;
            case AnimFlags.ss_rt | AnimFlags.stop:
                m_lastFlags = AnimFlags.idle1;
                break;
            //end if ss_rt

            case AnimFlags.run | AnimFlags.ss_lt:
                if (Input.GetKey(KeyCode.D))
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.lt;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.stop;
                }
                break;
            //end of transition forward to ss_lt
            case AnimFlags.run | AnimFlags.ss_rt:
                if (Input.GetKey(KeyCode.A))
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.rt;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.stop;
                }
                break;
            //end of transition forward to ss_rt
            case AnimFlags.ss_lt_run:
                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.rt;
                }
                else
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.stop;
                }
                break;
            //end of transition ss_lt to run
            case AnimFlags.ss_rt_run:
                if (Input.GetKey(KeyCode.W))
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.lt;
                }
                else
                {
                    m_lastFlags = AnimFlags.run | AnimFlags.stop;
                }
                break;
            //end of transition ss_rt to run
            case AnimFlags.run_bk | AnimFlags.ss_lt :
                if (Input.GetKey(KeyCode.D))
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.lt;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_lt | AnimFlags.stop;
                }
                break;
            //end of transition run_bk to ss_lt
            case AnimFlags.run_bk | AnimFlags.ss_rt:
                if (Input.GetKey(KeyCode.A))
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.rt;
                }
                else
                {
                    m_lastFlags = AnimFlags.ss_rt | AnimFlags.stop;
                }
                break;
            //end of transition run_bk to ss_rt
        } 
    }

    public void OnActionFrame(string param)
    {
        if (param != m_clipname)
        {
            return;
        }
        if(param.Contains("|DONOTUSE"))
        {
            Debug.Log(param);
            Debug.Break();
            return;
        }
        //if (param == m_clipname)
        {
            OnLastFrame();
        }
        PlayClip("KONOKO" + GetAnim());
        return;

        switch (param)
        {
           
            case "KONOKOidle1":
                if (Input.GetKey(KeyCode.W))
                {
                    PlayClip("KONOKOrun1stepa");
                }
                if (Input.GetKey(KeyCode.S))
                {
                    PlayClip("KONOKOrun_bk_1stepa");
                }

                if (Input.GetKey(KeyCode.A))
                {
                    PlayClip("KONOKOss_rt_1stepa");
                }
                if (Input.GetKey(KeyCode.D))
                {
                    PlayClip("KONOKOss_lt_1stepa");
                }
                break;
            #region run_forward
            case "KONOKOrunstop":
                PlayClip("KONOKOidle1");
                break;
            case "KONOKOrunstart":
                PlayClip("KONOKOrun_rt");
                break;

            case "KONOKOrun1stepa":
                if (Input.GetKey(KeyCode.W))
                {
                    PlayClip("KONOKOrunstart");
                }
                else
                {
                    PlayClip("KONOKOrun1stepb");
                }
                break;
            case "KONOKOrun1stepb": 
                PlayClip("KONOKOidle1");
                break;

            case "KONOKOrun_lt":
                PlayClip("KONOKOrun_rt");
            break;

            case "KONOKOrun_rt":
            if (Input.GetKey(KeyCode.W))
            {
                PlayClip("KONOKOrun_lt");
            }
            else
            {
                animation.CrossFade("KONOKOrunstop");
            }
            break;
            #endregion
            #region run_backward
            case "KONOKOrun_bk_stop":
            PlayClip("KONOKOidle1");
            break;
            case "KONOKOrun_bk_start":
            
            PlayClip("KONOKOrun_bk_rt");
            
            break;

            case "KONOKOrun_bk_1stepa":
            if (Input.GetKey(KeyCode.S))
            {
                PlayClip("KONOKOrun_bk_start");
            }
            else
            {
                PlayClip("KONOKOrun_bk_1stepb");
            }
            break;
            case "KONOKOrun_bk_1stepb":
            PlayClip("KONOKOidle1");
            break;

            case "KONOKOrun_bk_lt":
            PlayClip("KONOKOrun_bk_rt");
            break;

            case "KONOKOrun_bk_rt":
            if (Input.GetKey(KeyCode.S))
            {
                PlayClip("KONOKOrun_bk_lt");
            }
            else
            {
                animation.CrossFade("KONOKOrun_bk_stop");
            }
            break;
            #endregion
            #region run_left
            case "KONOKOss_rt_stop":
            PlayClip("KONOKOidle1");
            break;
            case "KONOKOss_rt_start":
            PlayClip("KONOKOss_rt_rt");
            break;
            
            case "KONOKOss_rt_1stepa":
            if (Input.GetKey(KeyCode.A))
            {
                PlayClip("KONOKOss_rt_start");
            }
            else
            {
                PlayClip("KONOKOss_rt_1stepb");
            }
            break;
            case "KONOKOss_rt_1stepb":
            PlayClip("KONOKOidle1");
            break;

            case "KONOKOss_rt_lt":
            PlayClip("KONOKOss_rt_rt");
            break;

            case "KONOKOss_rt_rt":
            if (Input.GetKey(KeyCode.A))
            {
                PlayClip("KONOKOss_rt_lt");
            }
            else
            {
                animation.CrossFade("KONOKOss_rt_stop");
            }
            break;
            #endregion
            #region run_right
            case "KONOKOss_lt_stop":
            PlayClip("KONOKOidle1");
            break;
            case "KONOKOss_lt_start":
            PlayClip("KONOKOss_lt_lt");
            break;

            case "KONOKOss_lt_1stepa":
            if (Input.GetKey(KeyCode.D))
            {
                PlayClip("KONOKOss_lt_start");
            }
            else
            {
                PlayClip("KONOKOss_lt_1stepb");
            }
            break;
            case "KONOKOss_lt_1stepb":
            PlayClip("KONOKOidle1");
            break;

            case "KONOKOss_lt_lt":
            PlayClip("KONOKOss_lt_rt");
            break;

            case "KONOKOss_lt_rt":
            if (Input.GetKey(KeyCode.D))
            {
                PlayClip("KONOKOss_lt_lt");
            }
            else
            {
                animation.CrossFade("KONOKOss_lt_stop");
            }
            break;
            #endregion
        }
        //Debug.Log("animation finish:" + param);
    }

	// Use this for initialization
	void Start () 
    {
        Vector3 angles = transform.eulerAngles;
        m_camera = GetComponentInChildren<Camera>().transform;
        m_camera.transform.parent = null;
        m_x = angles.y;
        m_y = angles.x;
	}

    public float m_upperVector = 0;
    public Vector3 m_motionVector;

    public Vector3 MotionVector
    {
        get
        {
            return m_motionVector;
        }
        set 
        { 
            m_motionVector = value;
        }
    }

    Vector3 m_clipDirection = Vector3.zero;
    Vector3 m_lastClipDirection = Vector3.zero;
    AnimationState from;
    AnimationState to;
    float _from_time;
    float startTime;
    string adds2 = "";

    void PlayClip(string name)
    {
        if (name != m_clipname && !animation.IsPlaying(name) && NewBehaviourScript.m_events.ContainsKey(name))
        {
            NewBehaviourScript.m_events[name].stringParameter = NewBehaviourScript.m_events[name].stringParameter.Replace("|DONOTUSE", "");
            Oni.Totoro.AnimationState old = 0;
            Oni.Totoro.AnimationState @new = 0;
            if (!string.IsNullOrEmpty(m_clipname))
            {
                NewBehaviourScript.m_events[m_clipname].stringParameter += "|DONOTUSE";
                from = animation[m_clipname];
                to = animation[name];
                old = NewBehaviourScript.m_anims[m_clipname].ToState;
                
            }

            m_clipname = name;
            @new = NewBehaviourScript.m_anims[m_clipname].FromState;
            adds2 = "mixstate: " + (old != @new).ToString();
            
            {
                if (old != @new)
                {
                    animation.CrossFade(name, 10f/animation[name].clip.frameRate);
                }
                else
                {
                    animation.CrossFade(name, 3f / animation[name].clip.frameRate);
                }
            }
        }
        else
        {
            if (!NewBehaviourScript.m_events.ContainsKey(name))
            {
                Debug.LogError("nave no clip " + name);
                Debug.LogError("clipdump:");
                string dump = "";

                foreach(string __name in NewBehaviourScript.m_events.Keys)
                {
                    dump += __name + "\n";
                }

                Debug.LogError(dump);
            }
        }
    }

    void OnColliosionEnter()
    {
        Debug.Log("enter!");
    }

    bool m_waitingForLanding = false;

    CollisionFlags m_lastCollisionFlags;
    float m_jumpStart = 0;
    public float m_travelDist = 0.5f;
     float m_travelSpd = 55;
    
    Vector3 m_rememberedSpeed = Vector3.zero;
    float m_x = 0;
    float m_y = 0;

    static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
	    return Mathf.Clamp (angle, min, max);
    }

    Quaternion m_targetAngle = Quaternion.identity;
    Quaternion m_interpAngle = Quaternion.identity;
    public float yMinLimit = -60;
    public float yMaxLimit = 70;
    public float ySpeed = 100;
    public float xSpeed = 100;
    Transform m_camera;
    public static Transform m_cameraOrientTarget;

    void Update()
    {   
        //GetComponentInChildren<Camera>().transform.localPosition = new Vector3(0, 25, -21);
        //GetComponentInChildren<Camera>().transform.LookAt(transform.position + Vector3.up * 15f);
        if (m_lockCursor)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                m_lockCursor = false;
                Screen.lockCursor = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                m_lockCursor = true;
                Screen.lockCursor = true;
            }
        }

        if (NewBehaviourScript.EndFlag)
        {
            CollisionFlags flags = GetComponentInChildren<CharacterController>().Move(transform.rotation * (m_motionVector + Physics.gravity + m_rememberedSpeed + new Vector3(0, m_jumpStart != 0 ? Mathf.Sqrt(Mathf.Abs(m_jumpVal - (Time.time - m_jumpStart))) * Mathf.Sign((m_jumpVal - (Time.time - m_jumpStart))) * (m_travelDist * (Input.GetKey(KeyCode.Space) ? 1.3f : 1)) : 0, 0)) * Time.deltaTime);
            //rigidbody.MovePosition(Vector3.up);
            //rigidbody.MovePosition(Vector3.down);
            {
                if(m_lockCursor)
                {
                    m_x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    m_y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                }

                m_interpAngle = Quaternion.Lerp(m_interpAngle, m_targetAngle, Time.deltaTime * 15f);
                m_y = ClampAngle(m_y, yMinLimit, yMaxLimit);
                Quaternion rotation = Quaternion.Euler(m_y, m_x, 0);
                Vector3 position = rotation * new Vector3(0, 0, -25f) + transform.position;
                m_camera.rotation = Quaternion.Euler(rotation.eulerAngles.x, 0, 0);
                transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y + m_interpAngle.eulerAngles.y, 0);
                m_camera.position = position;
                m_camera.LookAt(transform.position + new Vector3(0, 15, 0));
            }

            if ((flags & CollisionFlags.CollidedBelow) != 0)
            {
                if (m_waitingForLanding)
                {
                    OnLand();
                }
            }
            else
            {
                //if just lifted off

                if ((m_lastCollisionFlags & CollisionFlags.CollidedBelow) != 0 && (flags & CollisionFlags.CollidedBelow) == 0)
                {
                    if (m_jumpStart == 0)
                    {
                        m_jumpStart = Time.time - m_jumpVal;
                        m_rememberedSpeed = m_motionVector;
                        Debug.Log("FALL LIFTOFF");
                        m_waitingForLanding = true;
                        m_lastFlags = AnimFlags.jump | AnimFlags.idle;
                    }
                    //m_jumpStart = Time.time;
                }
            }

            m_lastCollisionFlags = flags;

            PlayClip("KONOKO" + GetAnim());
            if (animation["KONOKO" + GetAnim()] != null)
            {
                adds = animation["KONOKO" + GetAnim()].normalizedTime.ToString() + "\n" + (m_jumpStart != 0 ? m_upperVector * (Time.time - m_jumpStart) - 0.5f * (Time.time - m_jumpStart) * (Time.time - m_jumpStart) * 9.8f : 0).ToString();
            }
        }
    }

    Vector2 m_scrollVal = Vector2.zero;

    public static List<AnimationClip> m_clips = new List<AnimationClip>();
    string m_clipname = "";
    string adds = "";
	// Update is called once per frame
    void OnGUI()
    {
        GUILayout.Space(50);
        GUILayout.Label(m_clipname + "\n" + adds + "\n" + adds2 + "\nadditional angle:" + m_targetAngle.eulerAngles);

        return;
        if (animation != null)
        {
            m_scrollVal = GUILayout.BeginScrollView(m_scrollVal, GUIStyle.none);
            foreach (AnimationClip clip in m_clips)
            {
                if (GUILayout.Button(clip.name))
                {
                    PlayClip(clip.name);
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
