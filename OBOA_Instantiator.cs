using UnityEngine;
using System.Collections;
using Round2;
using System.Collections.Generic;

public class OBOA_Instantiator : MonoBehaviour 
{
    public OBOAObject m_obj;
    public Texture2D m_tex;
    public Texture2D m_tex2;
    public string m_id;
    public string flags;
    public UnityEngine.Quaternion __q;

    IEnumerator ICOR()
    {
        yield return new WaitForSeconds(15);
        m_tex = m_obj.Geometry.M3GA.Geometries.Link.M3GM.Texture.TXMP.UnityTexture;
        //Debug.LogError(NewBehaviourScript.m_textureCache.ContainsKey(m_obj.Geometry.M3GA.Geometries.Link.M3GM.Texture.TXMP.id), this);
        NewBehaviourScript.m_textureCache.TryGetValue(m_obj.Geometry.M3GA.Geometries.Link.M3GM.Texture.TXMP.id, out m_tex2);
    }

    void MakeFromM3GM(M3GM data)
    {
        int[] indiсes = data.TriangleStrips.IDXA.DecodeForM3GM_1();
        UnityEngine.Vector3[] planeNormals = new List<Round2.Vector3>(data.Points.PNTA.Positions).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        UnityEngine.Vector3[] pts = new List<Round2.Vector3>(data.Points.PNTA.Positions).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        UnityEngine.Vector2[] uvs = new List<Round2.Vector2>(data.TextureCoordinates.TXCA.TexCoords).ConvertAll<UnityEngine.Vector2>(u => u.Value).ToArray();
        UnityEngine.Vector3[] normals = new List<Round2.Vector3>(data.VertexNormals.VCRA.Normals).ConvertAll<UnityEngine.Vector3>(u => u.Value).ToArray();
        GameObject l_ch = GameObject.CreatePrimitive(PrimitiveType.Plane);
        l_ch.renderer.material.shader = Shader.Find("TwoSidedDiffuse");
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
            StartCoroutine(ICOR());
            m_id = m_obj.ScriptId.ToString();
            
            MakeFromM3GM(m_obj.Geometry.M3GA.Geometries.Link.M3GM);
        }
        else
        {
            Destroy(gameObject);
        }
	}
}
