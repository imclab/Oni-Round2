using UnityEngine;
using System.Collections;
using Round2;
using System.Collections.Generic;

public interface IOnTriggerHit
{
    System.Action OnHit { get; }
}

public class OBOA_Instantiator : MonoBehaviour, IOnTriggerHit
{
    static Dictionary<int, List<OBOA_Instantiator>> m_doorlist = new Dictionary<int, List<OBOA_Instantiator>>();
    public OBOAObject m_obj;
    public Texture2D m_tex;
    public Texture2D m_tex2;
    public int m_id;
    public string flags;
    public UnityEngine.Quaternion __q;
    public int door_id;

    void OnTriggerHit(Collider other)
    {
        Debug.LogError(other, other);
    }

    public void ImplementDoorClassCall(Round2.BINADOOR doorclass)
    {
    }

    public System.Action m_onTriggerHit = () => { };

    public static void InitializeFrom(Round2.BINADOOR door)
    {
        if (m_doorlist.ContainsKey(door._OSD.DoorId))
        {
            Round2.DOOR l_d = Round2.DOOR.m_doorClasses[door._OSD.Class];
            
            foreach (OBOA_Instantiator iinst in m_doorlist[door._OSD.DoorId])
            {
                SphereCollider sc = iinst.gameObject.AddComponent<SphereCollider>();
                sc.radius = Mathf.Sqrt(door._OSD.SquaredActivationRadius) / 2f;
                sc.isTrigger = true;
                Animation l_a = iinst.GetComponentInChildren<Animation>();
                l_a.AddClip(l_d.Animation.OBAN.Clip(true), "door");

                iinst.m_onTriggerHit = () => 
                {
                    foreach (OBOA_Instantiator _iinst in m_doorlist[door._OSD.DoorId])
                    {
                        _iinst.GetComponentInChildren<Animation>().Play("door");
                    }
                };
            }
        }
        else
        {
            Debug.LogError("INVALID DOOR ID:" + door._OSD.DoorId);
        }
    }

    public static void DoorClassCall(Round2.BINADOOR doorclass)
    {
        if (m_doorlist.ContainsKey(doorclass._OSD.DoorId))
        {
            foreach (OBOA_Instantiator iinst in m_doorlist[doorclass._OSD.DoorId])
            {
                iinst.ImplementDoorClassCall(doorclass);
            }
        }
        else
        {
            Debug.LogError("Improper doorId or ONOAid");
        }
    }

    void MakeFromM3GM(M3GM data)
    {
        int[] indiсes = data.TriangleStrips.IDXA.DecodeForM3GM_1();
        
        if(this.flags.Contains("FaceCollision"))
        {
            List<int> indcs = new List<int>();
            List<int> _3inds = new List<int>();

            foreach(int _m in indiсes)
            {
                _3inds.Add(_m);

                if(_3inds.Count > 2)
                {
                    _3inds.Reverse();
                    indcs.AddRange(_3inds);
                    _3inds.Clear();
                }
            }

            indiсes = indcs.ToArray();
        }

        UnityEngine.Vector3[] planeNormals = new List<Round2.Vector3>(data.Points.PNTA.Positions).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        UnityEngine.Vector3[] pts = new List<Round2.Vector3>(data.Points.PNTA.Positions).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        UnityEngine.Vector2[] uvs = new List<Round2.Vector2>(data.TextureCoordinates.TXCA.TexCoords).ConvertAll<UnityEngine.Vector2>(u => u.Value).ToArray();
        UnityEngine.Vector3[] normals = new List<Round2.Vector3>(data.VertexNormals.VCRA.Normals).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        GameObject l_ch = GameObject.CreatePrimitive(PrimitiveType.Plane);
        l_ch.AddComponent<Animation>();
        Destroy(l_ch.collider);
        ;
        l_ch.renderer.material.mainTexture = m_tex;
        l_ch.transform.parent = transform;
        l_ch.transform.localPosition = UnityEngine.Vector3.zero;
        l_ch.transform.localRotation = UnityEngine.Quaternion.identity;
        l_ch.transform.localScale = UnityEngine.Vector3.one;
        Mesh m = new Mesh();
        m.vertices = pts;
        m.uv = uvs;
        m.normals = normals;
        m.triangles = indiсes;
        m.RecalculateBounds();
        m.RecalculateNormals();
        l_ch.GetComponent<MeshFilter>().mesh = m;
        l_ch.AddComponent<MeshCollider>().mesh = m;

        {

            float[] l_trs = new List<string>(m_obj.Transform.Split(" ".ToCharArray())).ConvertAll(u => float.Parse(u)).ToArray();

            Oni.Matrix l_trsm = new Oni.Matrix
               (
                    l_trs[0],
                    l_trs[1],
                    l_trs[2],
                    0,
                    l_trs[3],
                    l_trs[4],
                    l_trs[5],
                    0,
                    l_trs[6],
                    l_trs[7],
                    l_trs[8],
                    0,
                    l_trs[9],
                    l_trs[10],
                    l_trs[11],
                    1
                );

            Oni.Vector3 scale;
            Oni.Vector3 trans;
            Oni.Quaternion rot;
            //l_trsm.Transpose();
            l_trsm.Decompose(out scale, out rot, out trans);
            transform.localScale = new UnityEngine.Vector3(scale.X, scale.Y, scale.Z);
            transform.position = new UnityEngine.Vector3(trans.X, trans.Y, trans.Z);
            transform.rotation = new UnityEngine.Quaternion(rot.X, rot.Y, rot.Z, rot.W);

        }
        if (m_obj.Animation.OBAN != null && m_obj.Animation.OBAN.KeyFrames != null)
        {
            float[] l_trs = new List<string>(m_obj.Animation.OBAN.InitialTransform.Split(" ".ToCharArray())).ConvertAll(u => float.Parse(u)).ToArray();
            float[] l_trss = new List<string>(m_obj.Animation.OBAN.BaseTransform.Split(" ".ToCharArray())).ConvertAll(u => float.Parse(u)).ToArray();
            
            Oni.Matrix l_trsm = new Oni.Matrix
               (
                    l_trs[0],
                    l_trs[1],
                    l_trs[2],
                    0,
                    l_trs[3],
                    l_trs[4],
                    l_trs[5],
                    0,
                    l_trs[6],
                    l_trs[7],
                    l_trs[8],
                    0,
                    l_trs[9],
                    l_trs[10],
                    l_trs[11],
                    1
                );

            Oni.Matrix l_trsms = new Oni.Matrix
               (
                    l_trss[0],
                    l_trss[1],
                    l_trss[2],
                    0,
                    l_trss[3],
                    l_trss[4],
                    l_trss[5],
                    0,
                    l_trss[6],
                    l_trss[7],
                    l_trss[8],
                    0,
                    l_trss[9],
                    l_trss[10],
                    l_trss[11],
                    1
                );

            Oni.Vector3 scale;
            Oni.Vector3 trans;
            Oni.Quaternion rot;
            //l_trsm.Transpose();
            l_trsm.Decompose(out scale, out rot, out trans);
            transform.localScale = new UnityEngine.Vector3(scale.X, scale.Y, scale.Z);
            transform.position = new UnityEngine.Vector3(trans.X, trans.Y, trans.Z);
            transform.rotation = __q = new UnityEngine.Quaternion(rot.X, rot.Y, rot.Z, rot.W);
        }
    }

	void Start () 
    {
        if (m_obj.Geometry != null && m_obj.Geometry.M3GA != null)
        {
            flags = m_obj.Flags;
            //Debug.Log(m_obj.Geometry.M3GA.id);
            m_tex = m_obj.Geometry.M3GA.Geometries.Link.M3GM.Texture.TXMP.UnityTexture;
            m_id = m_obj.Geometry.M3GA.Geometries.Link.M3GM.TriangleStrips.IDXA.id;
            door_id = m_obj.DoorId << 24 >> 24;

            if (door_id >= 0)
            {
                if (!m_doorlist.ContainsKey(door_id))
                {
                    m_doorlist.Add(door_id, new List<OBOA_Instantiator>());
                }

                m_doorlist[door_id].Add(this);
            }

            MakeFromM3GM(m_obj.Geometry.M3GA.Geometries.Link.M3GM);
        }
        else
        {
            Destroy(gameObject);
        }
	}

    void OnTriggerEnter(Collider other)
    {
        foreach (MonoBehaviour mb in other.gameObject.GetComponents<MonoBehaviour>())
        {
            if (mb is ICHR)
            {
                (mb as ICHR).OnDoorInteract(this);
            }
        }
    }

    public System.Action OnHit
    {
        get 
        {
            return m_onTriggerHit; 
        }
    }
}
