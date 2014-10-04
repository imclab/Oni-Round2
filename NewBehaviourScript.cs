using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Oni.Xml;
using Oni.Metadata;
using System.Xml.Serialization;

public interface ICHR
{
    void OnDoorInteract(OBOA_Instantiator door);
}

public class NewBehaviourScript : MonoBehaviour, ICHR
{
    public MeshFilter mf;
    public SkinnedMeshRenderer sr;

    void ConstructBodypart(Oni.Motoko.Geometry geo, Transform obj, Oni.Motoko.Texture texture)
    {
        Mesh m = new Mesh();
        m.vertices = new List<Oni.Vector3>(geo.Points).ConvertAll<UnityEngine.Vector3>(u => Quaternion.Euler(0,0,0) * new Vector3(u.X, u.Y, u.Z)).ToArray();
        List<int> m_trisres = new List<int>();
        List<int> m_trisl = new List<int>();

        foreach (int i in geo.Triangles)
        {
            m_trisl.Add(i);

            if (m_trisl.Count == 3)
            {
                //m_trisl.Reverse();
                m_trisres.AddRange(m_trisl);
                m_trisl.Clear();
            }
        }

        m.triangles = m_trisres.ToArray();
        m.normals = new List<Oni.Vector3>(geo.Normals).ConvertAll<UnityEngine.Vector3>(u => new Vector3(u.X, u.Y, u.Z)).ToArray();
        m.uv = new List<Oni.Vector2>(geo.TexCoords).ConvertAll<UnityEngine.Vector2>(u => new Vector2(u.X, u.Y)).ToArray();
        obj.gameObject.AddComponent<MeshFilter>().mesh = m;
        Texture2D t = new Texture2D(texture.Width, texture.Height, TextureFormat.RGBA32, true);
        Oni.Imaging.Surface s = texture.Surfaces[0].Convert(Oni.Imaging.SurfaceFormat.RGBA);
        List<byte> colorBytes = new List<byte>();
        List<Color32> colors = new List<Color32>();

        foreach (byte b in s.Data)
        {
            colorBytes.Add(b);

            if (colorBytes.Count == 4)
            {
                colors.Add(new Color32(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]));
                colorBytes.Clear();
            }
        }

        t.SetPixels32(colors.ToArray());
        
        t.Apply(true);
        (obj.gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"))).mainTexture = t ;
    }

    void ValidateChildrens(Oni.Totoro.BodyNode parentNode, Dictionary<string, Transform> nameToTransform, string rootNodeName)
    {
        foreach (Oni.Totoro.BodyNode child in parentNode.Children)
        {
            ValidateChildrens(child, nameToTransform, rootNodeName);
            nameToTransform[child.Name].parent = nameToTransform[parentNode.Name];
            nameToTransform[child.Name].localPosition = Quaternion.Euler(0,0,0) * new Vector3(child.Translation.X,  child.Translation.Y,  child.Translation.Z);
        }
    }

    string GetPath(Transform t, string node = "")
    {
        if (t.parent != null)
        {
            return  GetPath(t.parent, t.name) + (!string.IsNullOrEmpty(node) ? "/" + node : "");
        }
        else
        {
            return node;
        }
    }

    private static float[] MakeRotationCurveContinuous(float[] curve)
    {
        for (int i = 1; i < (int)curve.Length; i++)
        {
            float single = curve[i - 1];
            float single1 = curve[i];
            if (Mathf.Abs(single1 - single) > 180f)
            {
                single1 = (single1 <= single ? single1 + 360f : single1 - 360f);
                curve[i] = single1;
            }
        }

        return curve;
    }

    Keyframe[] FinalizeCurves(Keyframe[] kf, float[] poss)
    {
        Debug.Log(kf.Length + ":" + poss.Length);
        for (int i = 0; i < kf.Length; i++)
        {
            kf[i].value = poss[i];
        }

        return kf;
    }

    class MaterialData
    {
        public List<Vector3> m_vrts = new List<Vector3>();
        public int m_objId = -1;
        public Dictionary<int, int> m_oldToNewInds = new Dictionary<int, int>();
        List<int> m_inds = new List<int>();
        public List<Vector2> m_uvs = new List<Vector2>();

        public class Vertex
        {
            public int index = -1;
            public Vector2 uv = Vector2.zero;
            public Vector3 pos = Vector3.zero;
        }

        public static string mMmea = "";

        List<Vertex> m_ivrts = new List<Vertex>();

        public void EmitTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            m_ivrts.Add(v1);

            if(v1.index == -1)
            {
                v1.index = m_ivrts.Count - 1;
            }

            m_ivrts.Add(v2);

            if(v2.index == -1)
            {
                v2.index = m_ivrts.Count - 1;
            }

            m_ivrts.Add(v3);

            if(v3.index == -1)
            {
                v3.index = m_ivrts.Count - 1;
            }
        }

        public int[] GetTriangleStrip()
        {
            return m_ivrts.ConvertAll<int> ( u=> u.index).ToArray();
        }

        public Vector3[] GetVertexStrip()
        {
            return m_ivrts.ConvertAll<Vector3> ( u=> u.pos).ToArray();
        }

        public Vector2[] GetUvStrip()
        {
            return m_ivrts.ConvertAll<Vector2> ( u=> u.uv).ToArray();
        }
    }

    static GameObject m_levelRoot;
    public Shader m_c1;

    void InstallAKEV(Oni.InstanceDescriptor AKEV )
    {
        GameObject go = m_levelRoot = new GameObject(AKEV.Name);
        Oni.Akira.PolygonMesh mesh = Oni.Akira.AkiraDatReader.Read(AKEV);
        Mesh floorCollider = new Mesh();
        
        Debug.Log(mesh.Doors.Count + ">>>>>>>>>>>>>>>>>");//door amount
        {
            List<Vector3> floorV = new List<Vector3>();
            List<int> floorInds = new List<int>();

            foreach (Oni.Akira.Room r in mesh.Rooms)
            {
                
                {
                    foreach (Oni.Vector3[] _v in r.GetFloorPolygons())
                    {
                        floorV.Add(new Vector3(_v[0].X, _v[0].Y, _v[0].Z));
                        floorInds.Add(floorV.Count - 1);
                        floorV.Add(new Vector3(_v[1].X, _v[1].Y, _v[1].Z));
                        floorInds.Add(floorV.Count - 1);
                        floorV.Add(new Vector3(_v[2].X, _v[2].Y, _v[2].Z));
                        floorInds.Add(floorV.Count - 1);

                        floorV.Add(new Vector3(_v[0].X, _v[1].Y, _v[1].Z));
                        floorInds.Add(floorV.Count - 1);
                        floorV.Add(new Vector3(_v[2].X, _v[2].Y, _v[2].Z));
                        floorInds.Add(floorV.Count - 1);
                        floorV.Add(new Vector3(_v[3].X, _v[3].Y, _v[3].Z));
                        floorInds.Add(floorV.Count - 1);
                    }
                }
            }

            floorCollider.vertices = floorV.ToArray();
            floorCollider.triangles = floorInds.ToArray();
            floorCollider.RecalculateBounds();
            go.AddComponent<MeshCollider>().mesh = floorCollider;
            //go.AddComponent<MeshFilter>().mesh = floorCollider;
            //go.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
        }

        mesh.DoLighting();
        Debug.Log(mesh.Normals.Count);
        List<Vector3> normals = mesh.Normals.ConvertAll<Vector3>(u => new Vector3(u.X, u.Y, u.Z));
        List<Vector3> vrts = mesh.Points.ConvertAll<Vector3>(u => new Vector3(u.X, u.Y, u.Z));
        List<Vector2> uvs = new List<Vector2>(new Vector2[vrts.Count]);
        Dictionary<string, Dictionary<Oni.Akira.Material, MaterialData>> dataDict = new Dictionary<string, Dictionary<Oni.Akira.Material, MaterialData>>();

        if (normals.Count < vrts.Count)
        {
            normals.AddRange(new Vector3[vrts.Count - normals.Count]);
        }

        List<int> trs = new List<int>();
        Oni.Akira.Material m = null;
        Dictionary<string, int> nameToId = new Dictionary<string, int>();
        
        foreach (Oni.Akira.Polygon mp in mesh.Polygons)
        {
            if ((mp.Flags & Oni.Akira.GunkFlags.DoorFrame) != 0 || (mp.Flags & Oni.Akira.GunkFlags.Furniture) != 0 || m_objectONOAs.ContainsKey(mp.ObjectId) /*i bet, i don't need onoas statics*/)
            {
                continue;
            }

            if (m_objectONOAs.ContainsKey(mp.ObjectId))
            {
                //Debug.Log("have ONOA for " + mp.ObjectId + " AKA " + mp.ObjectName);
            }
            if (!dataDict.ContainsKey(mp.ObjectName))
            {
                dataDict.Add(mp.ObjectName, new Dictionary<Oni.Akira.Material, MaterialData>());
            }

            if (!dataDict[mp.ObjectName].ContainsKey(mp.Material))
            {
                dataDict[mp.ObjectName].Add(mp.Material, new MaterialData());
            }

            MaterialData md = dataDict[mp.ObjectName][mp.Material];
            if (md.m_objId == -1)
            {
                md.m_objId = mp.ObjectId;
            }

            switch (mp.Edges.Length)
            {
                case 3:
                    uvs[mp.Edges[0].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[0]].X, mesh.TexCoords[mp.TexCoordIndices[0]].Y);
                    uvs[mp.Edges[1].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[1]].X, mesh.TexCoords[mp.TexCoordIndices[1]].Y);
                    uvs[mp.Edges[2].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[2]].X, mesh.TexCoords[mp.TexCoordIndices[2]].Y);

                    md.EmitTriangle(
                        new MaterialData.Vertex() { pos = vrts[mp.Edges[0].Point0Index], uv = uvs[mp.Edges[0].Point0Index] },
                        new MaterialData.Vertex() { pos = vrts[mp.Edges[1].Point0Index], uv = uvs[mp.Edges[1].Point0Index] },
                        new MaterialData.Vertex() { pos = vrts[mp.Edges[2].Point0Index], uv = uvs[mp.Edges[2].Point0Index] }
                        );
                    break;
                case 4:

                    int[] indexes = new int[8];
                    indexes[0] = mp.Edges[0].Point0Index;
                    indexes[1] = mp.Edges[0].Point1Index;
                    indexes[2] = mp.Edges[1].Point0Index;
                    indexes[3] = mp.Edges[1].Point1Index;
                    indexes[4] = mp.Edges[2].Point0Index;
                    indexes[5] = mp.Edges[2].Point1Index;
                    indexes[6] = mp.Edges[3].Point0Index;
                    indexes[7] = mp.Edges[3].Point1Index;

                    uvs[mp.Edges[0].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[0]].X, mesh.TexCoords[mp.TexCoordIndices[0]].Y);
                    uvs[mp.Edges[1].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[1]].X, mesh.TexCoords[mp.TexCoordIndices[1]].Y);
                    uvs[mp.Edges[2].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[2]].X, mesh.TexCoords[mp.TexCoordIndices[2]].Y);
                    uvs[mp.Edges[3].Point0Index] = new Vector2(mesh.TexCoords[mp.TexCoordIndices[3]].X, mesh.TexCoords[mp.TexCoordIndices[3]].Y);

                    /*TODO: i guess, here will be added some extra polygons. Needs to be fixed!*/

                    md.EmitTriangle(
                        new MaterialData.Vertex() { pos = vrts[indexes[0]], uv = uvs[indexes[0]] },
                        new MaterialData.Vertex() { pos = vrts[indexes[1]], uv = uvs[indexes[1]] },
                        new MaterialData.Vertex() { pos = vrts[indexes[3]], uv = uvs[indexes[3]] }
                        );

                    md.EmitTriangle
                        (
                        new MaterialData.Vertex() { pos = vrts[indexes[4]], uv = uvs[indexes[4]] },
                        new MaterialData.Vertex() { pos = vrts[indexes[6]], uv = uvs[indexes[6]] },
                        new MaterialData.Vertex() { pos = vrts[indexes[7]], uv = uvs[indexes[7]] }
                        );
                    break;
            }
        }

        foreach (string objname in dataDict.Keys)
        {
            GameObject rgo = new GameObject(objname);
            rgo.transform.parent = m_levelRoot.transform;

            foreach (Oni.Akira.Material _m in dataDict[objname].Keys)
            {
                GameObject mgo = new GameObject(objname + "|" + _m.Name + "|Id::" + dataDict[objname][_m].m_objId.ToString());
                mgo.transform.parent = rgo.transform;
                Texture2D t2d = new Texture2D(_m.Image.Width, _m.Image.Height, TextureFormat.RGBA32, true);
                List<byte> colorBytes = new List<byte>();
                List<Color32> colors = new List<Color32>();

                foreach (byte b in _m.Image.Convert(Oni.Imaging.SurfaceFormat.RGBA).Data)
                {
                    colorBytes.Add(b);

                    if (colorBytes.Count == 4)
                    {
                        colors.Add(new Color32(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]));
                        colorBytes.Clear();
                    }
                }

                t2d.SetPixels32(colors.ToArray());
                t2d.Apply();
                Mesh me = new Mesh();
                me.vertices = dataDict[objname][_m].GetVertexStrip();
                me.uv = dataDict[objname][_m].GetUvStrip();

                //Debug.Log(me.uv[0].x + "|" + (me.uv[0].x / _m.Image.Width) +  "|" + me.uv[1].x + "|" + me.uv[3].x);
                me.triangles = dataDict[objname][_m].GetTriangleStrip();
                me.RecalculateNormals();
                me.RecalculateBounds();

                #region object flags
                bool showInvisible = Application.isEditor;
                bool transparent = (_m.Flags & Oni.Akira.GunkFlags.Transparent ) != Oni.Akira.GunkFlags.None; //(names & Names.Susan) != Names.None;
                bool noCollisions = (_m.Flags & Oni.Akira.GunkFlags.NoCollision) != Oni.Akira.GunkFlags.None; //(names & Names.Susan) != Names.None;
                bool invisble = (_m.Flags & Oni.Akira.GunkFlags.Invisible) != Oni.Akira.GunkFlags.None;
                bool twoSided = (_m.Flags & Oni.Akira.GunkFlags.TwoSided) != Oni.Akira.GunkFlags.None;
                bool isDoor = (_m.Flags & Oni.Akira.GunkFlags.DoorFrame) != Oni.Akira.GunkFlags.None;
                #endregion

                /*if (_m.IsMarker)
                {
                    MeshCollider mc =
                    mgo.AddComponent<MeshCollider>();
                    mc.mesh = me;
                    mc.gameObject.name = _m.Flags.ToString();
                    MeshFilter mmf = mgo.AddComponent<MeshFilter>();
                    (mgo.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"))).mainTexture = t2d;
                    mmf.mesh = me;
                }
                else*/
                {
                    if (twoSided)
                    {
                        List<int> newTris = new List<int>(me.triangles);
                        Stack<int> localTrisLst = new Stack<int>();

                        foreach (int trsi in me.triangles)
                        {
                            localTrisLst.Push(trsi);

                            if(localTrisLst.Count == 3)
                            {
                                newTris.Add(localTrisLst.Pop());
                                newTris.Add(localTrisLst.Pop());
                                newTris.Add(localTrisLst.Pop());
                            }
                        }

                        me.triangles = newTris.ToArray();
                        me.RecalculateNormals();
                    }

                    MeshFilter mmf = mgo.AddComponent<MeshFilter>();

                    //if (isLighting)
                    //{
                       
                    //}
                    //else
                    {
                        if (!invisble || showInvisible)
                        {
                            MeshRenderer mr = (mgo.AddComponent<MeshRenderer>());
                            (mr.material = new Material(Shader.Find(transparent ? "TransparentTwoSided" : "Diffuse"))).mainTexture = t2d;
                        }
                    }
                    mmf.mesh = me;
                    mmf.gameObject.name += "|" + _m.Flags.ToString(); 
                    
                    //if(!noCollisions)
                    {
                        MeshCollider mc =
                        mgo.AddComponent<MeshCollider>();
                        mc.mesh = me;
                        mc.isTrigger = noCollisions;
                    }
                }
            }
        }

        //vMesh.mesh = new Mesh() { vertices = vrts.ToArray(), triangles = trs.ToArray(), normals =  normals.ToArray(), uv = uvs.ToArray() };
        //lvMesh.mesh.RecalculateNormals();
        
        /*

        //mesh.Points
        foreach (Oni.Akira.Room r in mesh.Rooms)
        {
            GameObject newestGo = new GameObject(AKEV.Name + ":" + Random.value + ":floor");
            newestGo.transform.parent = go.transform;
            MeshFilter mf = newestGo.AddComponent<MeshFilter>();
            newestGo.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
            Mesh m = new Mesh();
            List<Oni.Vector3[]> polys = r.GetFloorPolygons();

            m.vertices = new List<Oni.Vector3>(polys[0]).ConvertAll<Vector3>(u => new Vector3(u.X, u.Y, u.Z)).ToArray();
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            m.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            m.uv = new Vector2[4];
            mf.mesh = m;

        }*/
    }

    [System.Flags]
    public enum OBANFlags : uint
    {
        None = 0u,
        NormalLoop = 1u,
        BackToBackLoop = 2u,
        RandomStartFrame = 4u,
        Autostart = 8u,
        ZAxisUp = 16u
    }

    public class OBAN
    {
        [System.Xml.Serialization.XmlAttribute]
        public int id;
        public OBANFlags Flag;
        public string InitialTransform;
        public float[] _InitialTransform;

        internal Oni.Matrix Trs
        {
            get
            {
                if (BaseTransform == null)
                {
                    _InitialTransform = new List<string>(InitialTransform.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                }

                return new Oni.Matrix(
                    InitialTransform[0],
                    InitialTransform[1],
                    InitialTransform[2],
                    InitialTransform[3],
                    InitialTransform[4],
                    InitialTransform[5],
                    InitialTransform[6],
                    InitialTransform[7],
                    InitialTransform[8],
                    InitialTransform[9],
                    InitialTransform[10],
                    InitialTransform[11],
                    InitialTransform[12],
                    InitialTransform[13],
                    InitialTransform[14],
                    InitialTransform[15]);
            }
        }

        public string BaseTransform;
        public float[] _BaseTransform;

        internal Oni.Matrix BaseTrs
        {
            get
            {
                if (_BaseTransform == null)
                {
                    _BaseTransform = new List<string>(BaseTransform.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                }

                return new Oni.Matrix(
                    BaseTransform[0],
                    BaseTransform[1],
                    BaseTransform[2],
                    BaseTransform[3],
                    BaseTransform[4],
                    BaseTransform[5],
                    BaseTransform[6],
                    BaseTransform[7],
                    BaseTransform[8],
                    BaseTransform[9],
                    BaseTransform[10],
                    BaseTransform[11],
                    BaseTransform[12],
                    BaseTransform[13],
                    BaseTransform[14],
                    BaseTransform[15]);
            }
        }

        public int FrameLength;
        public int FrameCount;
        public int HalfStopFrame;

        public class OBANKeyFrame
        {
            [System.Xml.Serialization.XmlArray("Rotation")]
            public float[] Rotation;

            public Quaternion _Rotation
            {
                get
                {
                    return new Quaternion(Rotation[0], Rotation[1], Rotation[2], Rotation[3]);
                }
            }

            //[System.Xml.Serialization.XmlArray("Translation")]
            public string Translation;
            public float[] _translation;

            public Vector3 _Translation
            {
                get
                {

                    if (_translation == null)
                    {
                        _translation = new List<string>(Translation.Split(" ".ToCharArray())).ConvertAll<float>(u => float.Parse(u)).ToArray();
                    }

                    return new Vector3(_translation[0], _translation[1], _translation[2]);
                }
            }

            public int Time;

            public float RealTime
            {
                get
                {
                    return Time * (1 / 60f);
                }
            }
        }

        [System.Xml.Serialization.XmlArray("KeyFrames")]
        public OBANKeyFrame[] KeyFrames;

        [System.Xml.Serialization.XmlIgnore]
        AnimationClip m_clip;

        public AnimationClip Clip
        {
            get
            {
                if (m_clip == null)
                {
                    List<Keyframe> rkFx = new List<Keyframe>();
                    List<Keyframe> rkFy = new List<Keyframe>();
                    List<Keyframe> rkFz = new List<Keyframe>();
                    List<Keyframe> rkFw = new List<Keyframe>();
                    List<Keyframe> kFx = new List<Keyframe>();
                    List<Keyframe> kFy = new List<Keyframe>();
                    List<Keyframe> kFz = new List<Keyframe>();

                    foreach (OBANKeyFrame okf in KeyFrames)
                    {
                        kFx.Add(new Keyframe(okf.RealTime, okf._Translation.x));
                        kFy.Add(new Keyframe(okf.RealTime, okf._Translation.y));
                        kFz.Add(new Keyframe(okf.RealTime, okf._Translation.z));

                        rkFx.Add(new Keyframe(okf.RealTime, okf._Rotation.x));
                        rkFy.Add(new Keyframe(okf.RealTime, okf._Rotation.y));
                        rkFz.Add(new Keyframe(okf.RealTime, okf._Rotation.z));
                        rkFw.Add(new Keyframe(okf.RealTime, okf._Rotation.w));
                    }

                    m_clip = new AnimationClip();
                    m_clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", new AnimationCurve(kFx.ToArray()));
                    m_clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", new AnimationCurve(kFy.ToArray()));
                    m_clip.SetCurve("", typeof(Transform), "m_LocalPosition.z", new AnimationCurve(kFz.ToArray()));
                }

                return m_clip;
            }
        }


    }

    void ReadOban(Oni.InstanceDescriptor oban)
    {
        ODump(oban);
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(ms);
        //new System.Xml.Serialization.XmlSerializer(typeof(OBAN)).Serialize( mr, new OBAN() { Flags = null });
        //Oni.Xml.TmbdXmlExporter.Export(reader, w);
        //new Oni.Xml.RawXmlExporter(reader, w);
        Oni.Action<Oni.InstanceDescriptor> ides = null;
        Oni.Xml.GenericXmlWriter.Write(w, ides = u => Oni.Xml.GenericXmlWriter.Write(w, ides, u), oban);
        w.Flush();
        w.Close();
        //mr.Seek(0, System.IO.SeekOrigin.Begin);
        ms.Seek(0, System.IO.SeekOrigin.Begin);
        OBAN ob;
        ob = (new System.Xml.Serialization.XmlSerializer(typeof(OBAN)).Deserialize(ms) as OBAN);
        Debug.Log(ob.id + "|" + ob.id.ToString("X"));
        //m_obans.Add((ob.id << 8 )| 1, ob);
        ms.Dispose();
        ms.Close();

       
        /*
        byte[] b_bytes = reader.ReadBytes(2);
        Debug.Log(((byte)oban.Name[0]).ToString("X"));
        Debug.Log(b_bytes[0].ToString("X") + b_bytes[1].ToString("X"));*/

        
    }

    public Dictionary<int, OBAN> m_obans = new Dictionary<int, OBAN>();

    public class LightData
    {
        public LightData() { }
        public LightData(Light l)
        {
            range = l.range;
            type = l.type.ToString();
            angle = l.spotAngle;
            color = l.color;
            intensity = l.intensity;
            bias = l.shadowBias;
            str = l.shadowStrength;
            softness = l.shadowSoftness;
            pos = l.transform.position;
            rot = l.transform.rotation;
        }
        public float range;
        public float angle;
        public string type;
        public Color color;
        public float intensity;
        public float bias;
        public float str;
        public float softness;
        public Vector3 pos;
        public Quaternion rot;

        public void CreateLight()
        {
            Light l = new GameObject().AddComponent<Light>();
            l.range = range;
            l.type = (LightType)System.Enum.Parse(typeof(LightType), type);
            l.spotAngle = angle;
            l.color = color;
            l.intensity = intensity;
            l.shadowBias = bias;
            l.shadowStrength = str;
            l.shadowSoftness = softness;
            l.transform.position = pos;
            l.transform.rotation = rot;
        }
    }

    string T_SCRIPT = @"env_show 101 0
env_show 102 0
env_show 103 0
env_show 104 0
env_show 105 0
env_show 106 0
env_show 107 0
env_show 108 0
env_show 109 0
env_setanim 101 ForkLift01
env_setanim 102 ForkLift02
env_setanim 103 ForkLift03
env_setanim 104 ForkLift04
env_setanim 105 ForkLift05
env_setanim 106 ForkLift06
env_setanim 107 ForkLift07
env_setanim 108 ForkLift08
env_setanim 109 ForkLift09
";
    void Start()
    {
        Application.RegisterLogCallback((u, u1, u2) =>
        {
            if (u2 == LogType.Exception)
            {
                excflag = true;
                exceptionstring = u + "\n" + u1;
            }
        });
        StartCoroutine(iStart());
        this.GetComponent<CharacterController>().detectCollisions = true;
    }

    [System.Serializable]
    public class IDXA
    {
        [System.Xml.Serialization.XmlArray("Indices" )]
        [System.Xml.Serialization.XmlArrayItem(ElementName = "Int32")]
        public System.Int32[] Indices;
        [System.Xml.Serialization.XmlAttribute("id")]
        public System.Int32 ObjectID;

        public override string ToString()
        {
            return "[IDXA] :: [id = " + ObjectID.ToString() + ", Indices count:" + Indices.Length + "]";
        }
    }

    [System.Serializable]
    public class ONOAElement
    {
        public int ObjectId;
        [System.Xml.Serialization.XmlIgnore]
        public int _RealObjectId;

        public enum ObjectKind
        { 
            character = 0x01, 
            patrol_path,
            door,
            flag,
            furniture,
            particle,
            powerup,
            sound,
            trigger_volume,
            weapon,
            trigger,
            turret,
            console,
            combat,
            melee,
            neutral
        }

        public ObjectKind Kind
        {
            get
            {
                return (ObjectKind)(ObjectId ^ RealObjectId << 8);
            }
        }

        public System.Int32 RealObjectId
        {
            get
            {
                return ObjectId << 8 >> 8;
            }
        }


        public IDXA QuadList;
    }

    [System.Serializable]
    public class ONOA
    {
        public ONOAElement[] Elements;
    }

    public Dictionary<int, ONOAElement> m_objectONOAs = new Dictionary<int, ONOAElement>();
    public List<ONOAElement> onoalist = new List<ONOAElement>();

    [System.Serializable]
    public class OBOA
    {
        [System.Xml.Serialization.XmlArray("Objects")]
        public OBOAObject[] Objects;
    }

    public class _Geometry
    {
        public M3GA M3GA;
    }

    [System.Serializable]
    public class OBOAObject
    {
        public _Geometry Geometry;
        public OBAN Animation;
        public int ScriptId;
        public string Transform;

        public Vector3 _Position
        {
            get
            {
                string[] s = Position.Split(new char[]{' '});
                return new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
            }    
        }


        public string Position;
        public string Rotation;
        public float Scale;
    }

    public class M3GA
    {
        //[System.Xml.Serialization.XmlArray("Geometries")]
        public _Geometries Geometries;
    }

    public class _Geometries
    {
        public Link Link = new Link();
    }

    public class Link
    {
        public M3GM M3GM = new M3GM();
    }

    public class M3GM
    {
        public PNTA Points;
        public VCRA VertexNormals;
        public VCRA FaceNormals;
        public TXCA TextureCoordinates;
        public IDXA TriangleStrips;
        public IDXA FaceNormalIndices;
        public TXMP Texture;
    }


    public class PNTA
    { 
        
    }

    public class BoundingBox
    { 
        
    }

    public class BoundingSphere
    { 
        
    }

    public class VCRA
    {
        [System.Xml.Serialization.XmlArray("Normals")]
        public Vector3[] Normals;
    }

    public class TXCA
    {
        [System.Xml.Serialization.XmlArray("TexCoords")]
        public Vector2[] TexCoords;
    }

    public class TXMP
    {
        /*
            <TXMP id="21">
                    <Flags>HasMipMaps SwapBytes</Flags>
                    <Width>64</Width>
                    <Height>64</Height>
                    <Format>DXT1</Format>
                    <Animation></Animation>
                    <EnvMap></EnvMap>
                    <DataOffset>82272</DataOffset>
         */

        public string Flags;
        public int Width;
        public int Height;
        public string Format;
        public string Animation;
        public string EnvMap;
        public int DataOffset;

    }

    void OBinaryDump(Oni.InstanceDescriptor id)
    {
        switch (id.Template.Tag)
        {
            case Oni.TemplateTag.ONOA:
                try
                {
                    MemoryStream ms = ODump(id);
                    ms.Seek(0, SeekOrigin.Begin);

                    foreach(ONOAElement el in (new System.Xml.Serialization.XmlSerializer(typeof(ONOA)).Deserialize(ms) as ONOA).Elements)
                    {
                        
                        if (el.RealObjectId != -1)
                        {
                            //m_objectONOAs.Add(el.RealObjectId, el);
                        }
                    }
                }
                catch (System.Exception ee)
                { 
                   /* MemoryStream ms = new MemoryStream();
                    new System.Xml.Serialization.XmlSerializer(typeof(IDXA)).Serialize(ms, new IDXA() { Indices = new int[] {1,2,3}}); 
                    Debug.Log(System.Text.UTF8Encoding.UTF8.GetString(ms.GetBuffer()));
                  */  Debug.Log(ee.ToString());
                }
                break;
            case Oni.TemplateTag.IDXA:
                break;
                try
                {
                    MemoryStream ms = ODump(id);
                    ms.Seek(0, SeekOrigin.Begin);
                    Debug.Log(new System.Xml.Serialization.XmlSerializer(typeof(IDXA)).Deserialize(ms));
                }
                catch (System.Exception ee)
                { 
                    MemoryStream ms = new MemoryStream();
                    new System.Xml.Serialization.XmlSerializer(typeof(IDXA)).Serialize(ms, new IDXA() { Indices = new int[] {1,2,3}}); 
                    Debug.Log(System.Text.UTF8Encoding.UTF8.GetString(ms.GetBuffer()));
                    Debug.Log(ee.ToString());
                }
                break;
        }
    }

    void FileDump(Oni.InstanceFile _if)
    { 
        
    }

    MemoryStream ODump(Oni.InstanceDescriptor des, bool discwrite = false)
    {
        MemoryStream ms = new MemoryStream();
        System.Xml.XmlWriter w = System.Xml.XmlWriter.Create(ms);
        
        Oni.Action<Oni.InstanceDescriptor> ides = null;
        ides = u =>
            {
                try
                {
                    Oni.Xml.GenericXmlWriter.Write(w, ides, u);
                }
                catch (System.Exception ee)
                {
                    //Debug.Log(ee);
                    //Debug.Log(des.IsPlaceholder);
                }
            };
        Oni.Xml.GenericXmlWriter.Write(w, ides, des);
        w.Flush();
        if (discwrite)
        {
            System.IO.File.WriteAllBytes("D:\\odump.xml", ms.GetBuffer());
        }
        //Debug.Log(System.Text.UTF8Encoding.UTF8.GetString(ms.GetBuffer()));
        return ms;
    }

    public static Dictionary<int, Texture2D> m_textureCache = new Dictionary<int, Texture2D>();
    public static Dictionary<int, System.Func<Texture2D>> m_textureObtainer = new Dictionary<int, System.Func<Texture2D>>();

    internal Texture2D ObtainTXFrom(Oni.InstanceDescriptor txca)
    {
        //Debug.LogWarning(txca.Index + "][" + txca.Name);
        if (m_textureCache.ContainsKey(txca.Index))
        {
            return m_textureCache[txca.Index];
        }
        else
        {
            try
            {
                Oni.Motoko.Texture l_rtex = Oni.Motoko.TextureDatReader.Read(txca);
                Texture2D l_t = new Texture2D(l_rtex.Width, l_rtex.Height, TextureFormat.RGBA32, true);
                Oni.Imaging.Surface l_s = l_rtex.Surfaces[0].Convert(Oni.Imaging.SurfaceFormat.RGBA);
                List<byte> l_colorBytes = new List<byte>();
                List<Color32> l_colors = new List<Color32>();

                foreach (byte b in l_s.Data)
                {
                    l_colorBytes.Add(b);

                    if (l_colorBytes.Count == 4)
                    {
                        l_colors.Add(new Color32(l_colorBytes[0], l_colorBytes[1], l_colorBytes[2], l_colorBytes[3]));
                        l_colorBytes.Clear();
                    }
                }

                l_t.name = txca.Name;
                l_t.SetPixels32(l_colors.ToArray());
                l_t.Apply(true);
                m_textureCache.Add(txca.Index, l_t);
                //Debug.Log(l_t, l_t);
                return l_t;
            }
            catch (System.Exception ee)
            {
                Debug.LogError( "TXCA id : " + txca.Index + "\nName:" + txca.Name + "\n" + ee.ToString());
                return null;
            }

            //texture.Surfaces[0].Convert(Oni.Imaging.SurfaceFormat.RGBA);


            /*
                Texture2D t = new Texture2D(texture.Width, texture.Height, TextureFormat.RGBA32, true);
                Oni.Imaging.Surface s = texture.Surfaces[0].Convert(Oni.Imaging.SurfaceFormat.RGBA);
                List<byte> colorBytes = new List<byte>();
                List<Color32> colors = new List<Color32>();

                foreach (byte b in s.Data)
                {
                    colorBytes.Add(b);

                    if (colorBytes.Count == 4)
                    {
                        colors.Add(new Color32(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]));
                        colorBytes.Clear();
                    }
                }

                t.SetPixels32(colors.ToArray());
        
                t.Apply(true);
             */
        }
    }

    IEnumerator iStart()
    {
        Application.targetFrameRate = 400;
        yield return null;
        {
            foreach (string filename in System.IO.Directory.GetFiles(Application.streamingAssetsPath, "Light*.txt"))
            {
                System.IO.StreamReader _sr;
                (System.Xml.Serialization.XmlSerializer.FromTypes(new System.Type[] { typeof(LightData) })[0].Deserialize(_sr = System.IO.File.OpenText(filename)) as LightData).CreateLight();
                _sr.Close();
                Debug.Log(filename);
            }

            #region animlist
            /*
        //        "TRAMKONCOMbk_fw_kick";
//        "TRAMKONCOMblock1";
//        "TRAMKONCOMblock1_end";
//        "TRAMKONCOMblock1_id";
//        "TRAMKONCOMblock2";
//        "TRAMKONCOMblock2_end";
//        "TRAMKONCOMblock2_id";
//        "TRAMKONCOMcomb_k";
//        "TRAMKONCOMcomb_k_k";
//        "TRAMKONCOMcomb_k_k_k";
//        "TRAMKONCOMcomb_k_k_kfw";
//        "TRAMKONCOMcomb_p";
//        "TRAMKONCOMcomb_p_p";
//        "TRAMKONCOMcomb_p_p_k";
//        "TRAMKONCOMcomb_p_p_p";
//        "TRAMKONCOMcrouch2idlea";
//        "TRAMKONCOMcrouch2idleb";
//        "TRAMKONCOMcrouch_bk";
//        "TRAMKONCOMcrouch_fw";
//        "TRAMKONCOMcrouch_kick1";
//        "TRAMKONCOMcrouch_kick_fw";
//        "TRAMKONCOMcrouch_lt";
//        "TRAMKONCOMcrouch_punch1";
//        "TRAMKONCOMcrouch_punch_fw";
//        "TRAMKONCOMcrouch_rt";
//        "TRAMKONCOMcr_block1";
//        "TRAMKONCOMcr_block1_end";
//        "TRAMKONCOMcr_block1_id";
//        "TRAMKONCOMfallen_back";
//        "TRAMKONCOMfallen_front";
//        "TRAMKONCOMgetupfront_bk";
//        "TRAMKONCOMgetupfront_bk_cr";
//        "TRAMKONCOMgetupfront_fw";
//        "TRAMKONCOMgetupfront_fw_cr";
//        "TRAMKONCOMgetupfront_k_bk";
//        "TRAMKONCOMgetupfront_k_fw";
//        "TRAMKONCOMgetupfront_lt";
//        "TRAMKONCOMgetupfront_p_fw";
//        "TRAMKONCOMgetupfront_rt";
//        "TRAMKONCOMgetup_bk";
//        "TRAMKONCOMgetup_bk_crouch";
//        "TRAMKONCOMgetup_fw";
//        "TRAMKONCOMgetup_fw_crouch";
//        "TRAMKONCOMgetup_kick_bk";
//        "TRAMKONCOMgetup_kick_fw";
//        "TRAMKONCOMgetup_kick_fw2";
//        "TRAMKONCOMgetup_lt";
//        "TRAMKONCOMgetup_rt";
//        "TRAMKONCOMhit_body1";
//        "TRAMKONCOMhit_body2";
//        "TRAMKONCOMhit_body3";
//        "TRAMKONCOMhit_body_bk1";
//        "TRAMKONCOMhit_body_bk2";
//        "TRAMKONCOMhit_crouch1";
//        "TRAMKONCOMhit_crouch2";
//        "TRAMKONCOMhit_fallen1";
//        "TRAMKONCOMhit_fallen2";
//        "TRAMKONCOMhit_fallenfront1";
//        "TRAMKONCOMhit_fallenfront2";
//        "TRAMKONCOMhit_foot1";
//        "TRAMKONCOMhit_foot2";
//        "TRAMKONCOMhit_foot_bk1";
//        "TRAMKONCOMhit_foot_ouch";
//        "TRAMKONCOMhit_head1";
//        "TRAMKONCOMhit_head2";
//        "TRAMKONCOMhit_head3";
//        "TRAMKONCOMhit_head_bk1";
//        "TRAMKONCOMhit_head_bk2";
//        "TRAMKONCOMhit_jewels";
//        "TRAMKONCOMidle1";
//        "TRAMKONCOMidle2";
//        "TRAMKONCOMidle2croucha";
//        "TRAMKONCOMidle2crouchb";
//        "TRAMKONCOMjump_bk_idle";
//        "TRAMKONCOMjump_bk_land";
//        "TRAMKONCOMjump_fw_idle";
//        "TRAMKONCOMjump_fw_land";
//        "TRAMKONCOMjump_idle";
//        "TRAMKONCOMjump_land";
//        "TRAMKONCOMjump_land_hd";
//        "TRAMKONCOMjump_lt_idle";
//        "TRAMKONCOMjump_lt_land";
//        "TRAMKONCOMjump_rt_idle";
//        "TRAMKONCOMjump_rt_land";
//        "TRAMKONCOMkick_bk";
//        "TRAMKONCOMkick_fw";
//        "TRAMKONCOMkick_fw_heavy";
//        "TRAMKONCOMkick_heavy";
//        "TRAMKONCOMkick_low1";
//        "TRAMKONCOMkick_lt";
//        "TRAMKONCOMkick_rt";
//        "TRAMKONCOMland_dead";
//        "TRAMKONCOMlt_fw_kick";
//        "TRAMKONCOMpunch_bk";
//        "TRAMKONCOMpunch_fw";
//        "TRAMKONCOMpunch_heavy";
//        "TRAMKONCOMpunch_lowa";
//        "TRAMKONCOMpunch_lowb";
//        "TRAMKONCOMpunch_lt";
//        "TRAMKONCOMpunch_rt";
//        "TRAMKONCOMrt_fw_kick";
//        "TRAMKONCOMrun1stepa";
//        "TRAMKONCOMrun1stepb";
//        "TRAMKONCOMrunstart";
//        "TRAMKONCOMrunstop";
//        "TRAMKONCOMrun_bk_1stepa";
//        "TRAMKONCOMrun_bk_1stepb";
//        "TRAMKONCOMrun_bk_start";
//        "TRAMKONCOMrun_bk_stop";
//        "TRAMKONCOMrun_throw_bk";
//        "TRAMKONCOMrun_throw_bk_tgt";
//        "TRAMKONCOMrun_throw_fw";
//        "TRAMKONCOMrun_throw_fw_tgt";
//        "TRAMKONCOMrun_thw_fw_k";
//        "TRAMKONCOMrun_thw_fw_k_tgt";
//        "TRAMKONCOMrun_tkl_bk_p";
//        "TRAMKONCOMrun_tkl_bk_p_tgt";
//        "TRAMKONCOMss_lt_1stepa";
//        "TRAMKONCOMss_lt_1stepb";
//        "TRAMKONCOMss_lt_start";
//        "TRAMKONCOMss_lt_stop";
//        "TRAMKONCOMss_rt_1stepa";
//        "TRAMKONCOMss_rt_1stepb";
//        "TRAMKONCOMss_rt_start";
//        "TRAMKONCOMss_rt_stop";
//        "TRAMKONCOMstagger1";
//        "TRAMKONCOMstagger_bk1";
//        "TRAMKONCOMstartle_bk1";
//        "TRAMKONCOMstartle_bk2";
//        "TRAMKONCOMstartle_fw1";
//        "TRAMKONCOMstartle_lt1";
//        "TRAMKONCOMstartle_rt1";
//        "TRAMKONCOMstun2";
//        "TRAMKONCOMstun3";
//        "TRAMKONCOMsuper_kick";
//        "TRAMKONCOMsuper_punch";
//        "TRAMKONCOMtaunt1";
//        "TRAMKONCOMtaunt2";
//        "TRAMKONCOMthrow_bk";
//        "TRAMKONCOMthrow_bk_k";
//        "TRAMKONCOMthrow_bk_k_tgt";
//        "TRAMKONCOMthrow_bk_tgt";
//        "TRAMKONCOMthrow_fw_k";
//        "TRAMKONCOMthrow_fw_k_tgt";
//        "TRAMKONCOMthrow_fw_p";
//        "TRAMKONCOMthrow_fw_p_tgt";
//        "TRAMKONOKOacid";
//        "TRAMKONOKOact_give";
//        "TRAMKONOKOact_no1";
//        "TRAMKONOKOact_no3";
//        "TRAMKONOKOact_shout";
//        "TRAMKONOKOact_shrug1";
//        "TRAMKONOKOact_talk1";
//        "TRAMKONOKOact_talk2";
//        "TRAMKONOKOact_yes1";
//        "TRAMKONOKOblownup1";
//        "TRAMKONOKOblownup2";
//        "TRAMKONOKOblownup3";
//        "TRAMKONOKOblownup_bk1";
//        "TRAMKONOKOblownup_bk2";
//        "TRAMKONOKOcomb_crouch_idle";
//        "TRAMKONOKOcomb_cr_turn_lt";
//        "TRAMKONOKOcomb_cr_turn_rt";
//        "TRAMKONOKOcomb_cr_walk";
//        "TRAMKONOKOcomb_cr_walk_backwards";
//        "TRAMKONOKOcomb_turnlt";
//        "TRAMKONOKOcomb_turnrt";
//        "TRAMKONOKOconsole";
//        "TRAMKONOKOconsole_punch";
//        "TRAMKONOKOconsole_walk";
//        "TRAMKONOKOcrouch2idlea";
//        "TRAMKONOKOcrouch2idleb";
//        "TRAMKONOKOcrouch_idle";
//        "TRAMKONOKOcrouch_run_bk_lt";
//        "TRAMKONOKOcrouch_run_bk_rt";
//        "TRAMKONOKOcrouch_run_lt";
//        "TRAMKONOKOcrouch_run_rt";
//        "TRAMKONOKOcrouch_turn_lt";
//        "TRAMKONOKOcrouch_turn_rt";
//        "TRAMKONOKOcrouch_walk";
//        "TRAMKONOKOcrouch_walk_backwards";
//        "TRAMKONOKOcycle_ride";
//        "TRAMKONOKOendpowerup";
//        "TRAMKONOKOfallen_front";
//        "TRAMKONOKOgetup_bk";
//        "TRAMKONOKOgetup_fw";
//        "TRAMKONOKOgetup_lt";
//        "TRAMKONOKOgetup_rt";
//        "TRAMKONOKOhail";
//        "TRAMKONOKOidle1";
//        "TRAMKONOKOidle2";
//        "TRAMKONOKOidle2croucha";
//        "TRAMKONOKOidle2crouchb";
//        "TRAMKONOKOidle2sit";
//        "TRAMKONOKOidle_spec2";
//        "TRAMKONOKOidle_spec3";
//        "TRAMKONOKOjump_bk_crouch";
//        "TRAMKONOKOjump_bk_idle";
//        "TRAMKONOKOjump_bk_kick";
//        "TRAMKONOKOjump_bk_land";
//        "TRAMKONOKOjump_bk_land_bk";
//        "TRAMKONOKOjump_bk_punch";
//        "TRAMKONOKOjump_bk_start";
//        "TRAMKONOKOjump_crouch";
//        "TRAMKONOKOjump_flail";
//        "TRAMKONOKOjump_fw_crouch";
//        "TRAMKONOKOjump_fw_idle";
//        "TRAMKONOKOjump_fw_kick";
//        "TRAMKONOKOjump_fw_land";
//        "TRAMKONOKOjump_fw_land_fw";
//        "TRAMKONOKOjump_fw_punch";
//        "TRAMKONOKOjump_fw_start";
//        "TRAMKONOKOjump_idle";
//        "TRAMKONOKOjump_kick";
//        "TRAMKONOKOjump_land";
//        "TRAMKONOKOjump_land_fw";
//        "TRAMKONOKOjump_lt_crouch";
//        "TRAMKONOKOjump_lt_idle";
//        "TRAMKONOKOjump_lt_kick";
//        "TRAMKONOKOjump_lt_land";
//        "TRAMKONOKOjump_lt_land_lt";
//        "TRAMKONOKOjump_lt_punch";
//        "TRAMKONOKOjump_lt_start";
//        "TRAMKONOKOjump_punch";
//        "TRAMKONOKOjump_rt_crouch";
//        "TRAMKONOKOjump_rt_idle";
//        "TRAMKONOKOjump_rt_kick";
//        "TRAMKONOKOjump_rt_land";
//        "TRAMKONOKOjump_rt_land_rt";
//        "TRAMKONOKOjump_rt_punch";
//        "TRAMKONOKOjump_rt_start";
//        "TRAMKONOKOjump_start";
//        "TRAMKONOKOknockdown1";
//        "TRAMKONOKOknockdown2";
//        "TRAMKONOKOknockdown_bk1";
//        "TRAMKONOKOknockdown_bk2";
//        "TRAMKONOKOknockdown_foot1";
//        "TRAMKONOKOknockdown_foot_bk";
//        "TRAMKONOKOlev10_watch_run";
//        "TRAMKONOKOlev11_cnsl_idle";
//        "TRAMKONOKOlev11_cnsl_start";
//        "TRAMKONOKOlev11_cnsl_stop";
//        "TRAMKONOKOlev11_cnsl_type";
//        "TRAMKONOKOlev11_jump";
//        "TRAMKONOKOlev1_Outro2";
//        "TRAMKONOKOlev3_intro";
//        "TRAMKONOKOlev4_undress";
//        "TRAMKONOKOlev5_outro_jump";
//        "TRAMKONOKOlev6_dismount";
//        "TRAMKONOKOlev6_tracer";
//        "TRAMKONOKOlev7_blown";
//        "TRAMKONOKOlev9_outro_pissed";
//        "TRAMKONOKOpickup_cr";
//        "TRAMKONOKOpickup_mid";
//        "TRAMKONOKOpickup_stand";
//        "TRAMKONOKOpowerup";
//        "TRAMKONOKOrun1stepa";
//        "TRAMKONOKOrun1stepb";
//        "TRAMKONOKOrunstart";
//        "TRAMKONOKOrunstop";
//        "TRAMKONOKOrun_bk_1stepa";
//        "TRAMKONOKOrun_bk_1stepb";
//        "TRAMKONOKOrun_bk_kick";
//        "TRAMKONOKOrun_bk_lt";
//        "TRAMKONOKOrun_bk_punch";
//        "TRAMKONOKOrun_bk_rt";
//        "TRAMKONOKOrun_bk_slide";
//        "TRAMKONOKOrun_bk_slide_id";
//        "TRAMKONOKOrun_bk_slide_run";
//        "TRAMKONOKOrun_bk_ss_lt";
//        "TRAMKONOKOrun_bk_ss_rt";
//        "TRAMKONOKOrun_bk_start";
//        "TRAMKONOKOrun_bk_stop";
//        "TRAMKONOKOrun_kick";
//        "TRAMKONOKOrun_lt";
//        "TRAMKONOKOrun_punch";
//        "TRAMKONOKOrun_rt";
//        "TRAMKONOKOrun_slide";
//        "TRAMKONOKOrun_slide_crouch";
//        "TRAMKONOKOrun_slide_getup";
//        "TRAMKONOKOrun_slide_run";
//        "TRAMKONOKOrun_ss_lt";
//        "TRAMKONOKOrun_ss_rt";
//        "TRAMKONOKOsit2idle";
//        "TRAMKONOKOsit_idle1";
//        "TRAMKONOKOss_lt_1stepa";
//        "TRAMKONOKOss_lt_1stepb";
//        "TRAMKONOKOss_lt_kick";
//        "TRAMKONOKOss_lt_lt";
//        "TRAMKONOKOss_lt_punch";
//        "TRAMKONOKOss_lt_rt";
//        "TRAMKONOKOss_lt_run";
//        "TRAMKONOKOss_lt_slide";
//        "TRAMKONOKOss_lt_start";
//        "TRAMKONOKOss_lt_stop";
//        "TRAMKONOKOss_rt_1stepa";
//        "TRAMKONOKOss_rt_1stepb";
//        "TRAMKONOKOss_rt_kick";
//        "TRAMKONOKOss_rt_lt";
//        "TRAMKONOKOss_rt_punch";
//        "TRAMKONOKOss_rt_rt";
//        "TRAMKONOKOss_rt_run";
//        "TRAMKONOKOss_rt_slide";
//        "TRAMKONOKOss_rt_start";
//        "TRAMKONOKOss_rt_stop";
//        "TRAMKONOKOturn_left";
//        "TRAMKONOKOturn_right";
//        "TRAMKONOKOwalk_back_lt";
//        "TRAMKONOKOwalk_back_rt";
//        "TRAMKONOKOwalk_back_start_lt";
//        "TRAMKONOKOwalk_back_start_rt";
//        "TRAMKONOKOwalk_back_stop_lt";
//        "TRAMKONOKOwalk_back_stop_rt";
//        "TRAMKONOKOwalk_lt";
//        "TRAMKONOKOwalk_rt";
//        "TRAMKONOKOwalk_ss_lt";
//        "TRAMKONOKOwalk_ss_lt_start";
//        "TRAMKONOKOwalk_ss_lt_stop";
//        "TRAMKONOKOwalk_ss_rt";
//        "TRAMKONOKOwalk_ss_rt_start";
//        "TRAMKONOKOwalk_ss_rt_stop";
//        "TRAMKONOKOwalk_start_lt";
//        "TRAMKONOKOwalk_start_rt";
//        "TRAMKONOKOwalk_stop_lt";
//        "TRAMKONOKOwalk_stop_rt";
//        "TRAMKONOKOwatch_idle";
//        "TRAMKONOKOwatch_radio";
//        "TRAMKONOKOwatch_start";
//        "TRAMKONOKOwatch_stop";
//        "TRAMKONPANcrouch2idle";
//        "TRAMKONPANcrouch_idle1";
//        "TRAMKONPANcrouch_idle2";
//        "TRAMKONPANcrouch_idle3";
//        "TRAMKONPANcrouch_idle4";
//        "TRAMKONPANcrouch_turn_lt";
//        "TRAMKONPANcrouch_turn_rt";
//        "TRAMKONPANidle1";
//        "TRAMKONPANidle2";
//        "TRAMKONPANidle2crouch";
//        "TRAMKONPANidle3";
//        "TRAMKONPANidle4";
//        "TRAMKONPANturn_lt";
//        "TRAMKONPANturn_rt";
//        "TRAMKONPIScrouch2idlea";
//        "TRAMKONPIScrouch2idleb";
//        "TRAMKONPIScrouch_idle";
//        "TRAMKONPIScrouch_run_bk_lt";
//        "TRAMKONPIScrouch_run_bk_rt";
//        "TRAMKONPIScrouch_run_lt";
//        "TRAMKONPIScrouch_run_rt";
//        "TRAMKONPIScrouch_turn_lt";
//        "TRAMKONPIScrouch_turn_rt";
//        "TRAMKONPIScrouch_walk";
//        "TRAMKONPIScrouch_walk_backwards";
//        "TRAMKONPISfallen_back";
//        "TRAMKONPISidle1";
//        "TRAMKONPISidle2";
//        "TRAMKONPISidle2croucha";
//        "TRAMKONPISidle2crouchb";
//        "TRAMKONPISjump_bk_idle";
//        "TRAMKONPISjump_bk_land";
//        "TRAMKONPISjump_bk_land_bk";
//        "TRAMKONPISjump_bk_start";
//        "TRAMKONPISjump_fw_idle";
//        "TRAMKONPISjump_fw_land";
//        "TRAMKONPISjump_fw_land_fw";
//        "TRAMKONPISjump_fw_start";
//        "TRAMKONPISjump_idle";
//        "TRAMKONPISjump_land";
//        "TRAMKONPISjump_land_fw";
//        "TRAMKONPISjump_lt_crouch";
//        "TRAMKONPISjump_lt_idle";
//        "TRAMKONPISjump_lt_land";
//        "TRAMKONPISjump_lt_land_lt";
//        "TRAMKONPISjump_lt_start";
//        "TRAMKONPISjump_rt_crouch";
//        "TRAMKONPISjump_rt_idle";
//        "TRAMKONPISjump_rt_land";
//        "TRAMKONPISjump_rt_land_rt";
//        "TRAMKONPISjump_rt_start";
//        "TRAMKONPISjump_start";
//        "TRAMKONPISpickup_cr";
//        "TRAMKONPISpickup_mid";
//        "TRAMKONPISpickup_pis_cr";
//        "TRAMKONPISpickup_pis_stand";
//        "TRAMKONPISpickup_stand";
//        "TRAMKONPISrun1stepa";
//        "TRAMKONPISrun1stepb";
//        "TRAMKONPISrunstart";
//        "TRAMKONPISrunstop";
//        "TRAMKONPISrun_bk_1stepa";
//        "TRAMKONPISrun_bk_1stepb";
//        "TRAMKONPISrun_bk_lt";
//        "TRAMKONPISrun_bk_rt";
//        "TRAMKONPISrun_bk_ss_lt";
//        "TRAMKONPISrun_bk_ss_rt";
//        "TRAMKONPISrun_bk_start";
//        "TRAMKONPISrun_bk_stop";
//        "TRAMKONPISrun_jump";
//        "TRAMKONPISrun_jump_kick";
//        "TRAMKONPISrun_jump_lt";
//        "TRAMKONPISrun_jump_rt";
//        "TRAMKONPISrun_jump_run";
//        "TRAMKONPISrun_jump_stop";
//        "TRAMKONPISrun_kick_lt";
//        "TRAMKONPISrun_kick_rt";
//        "TRAMKONPISrun_lt";
//        "TRAMKONPISrun_rt";
//        "TRAMKONPISrun_slide";
//        "TRAMKONPISrun_slide_crouch";
//        "TRAMKONPISrun_slide_getup";
//        "TRAMKONPISrun_slide_run";
//        "TRAMKONPISrun_ss_lt";
//        "TRAMKONPISrun_ss_rt";
//        "TRAMKONPISss_lt_1stepa";
//        "TRAMKONPISss_lt_1stepb";
//        "TRAMKONPISss_lt_lt";
//        "TRAMKONPISss_lt_punch";
//        "TRAMKONPISss_lt_rt";
//        "TRAMKONPISss_lt_run";
//        "TRAMKONPISss_lt_slide";
//        "TRAMKONPISss_lt_start";
//        "TRAMKONPISss_lt_stop";
//        "TRAMKONPISss_rt_1stepa";
//        "TRAMKONPISss_rt_1stepb";
//        "TRAMKONPISss_rt_lt";
//        "TRAMKONPISss_rt_punch";
//        "TRAMKONPISss_rt_rt";
//        "TRAMKONPISss_rt_run";
//        "TRAMKONPISss_rt_slide";
//        "TRAMKONPISss_rt_start";
//        "TRAMKONPISss_rt_stop";
//        "TRAMKONPISstartle_bk1";
//        "TRAMKONPISstartle_fw1";
//        "TRAMKONPISstartle_lt1";
//        "TRAMKONPISstartle_rt1";
//        "TRAMKONPISthrow_bk";
//        "TRAMKONPISthrow_bk_tgt";
//        "TRAMKONPISthrow_fw_k";
//        "TRAMKONPISthrow_fw_k_tgt";
//        "TRAMKONPISthrow_fw_p";
//        "TRAMKONPISthrow_fw_p_tgt";
//        "TRAMKONPISturn_left";
//        "TRAMKONPISturn_right";
//        "TRAMKONPISwalk_back_lt";
//        "TRAMKONPISwalk_back_rt";
//        "TRAMKONPISwalk_back_start_lt";
//        "TRAMKONPISwalk_back_start_rt";
//        "TRAMKONPISwalk_back_stop_lt";
//        "TRAMKONPISwalk_back_stop_rt";
//        "TRAMKONPISwalk_lt";
//        "TRAMKONPISwalk_rt";
//        "TRAMKONPISwalk_ss_lt";
//        "TRAMKONPISwalk_ss_lt_start";
//        "TRAMKONPISwalk_ss_lt_stop";
//        "TRAMKONPISwalk_ss_rt";
//        "TRAMKONPISwalk_ss_rt_start";
//        "TRAMKONPISwalk_ss_rt_stop";
//        "TRAMKONPISwalk_start_lt";
//        "TRAMKONPISwalk_start_rt";
//        "TRAMKONPISwalk_stop_lt";
//        "TRAMKONPISwalk_stop_rt";
//        "TRAMKONPISwatch_idle";
//        "TRAMKONPISwatch_start";
//        "TRAMKONPISwatch_stop";
//        "TRAMKONRIFcomb_p";
//        "TRAMKONRIFcrouch2idlea";
//        "TRAMKONRIFcrouch2idleb";
//        "TRAMKONRIFcrouch_idle";
//        "TRAMKONRIFcrouch_run_bk_lt";
//        "TRAMKONRIFcrouch_run_bk_rt";
//        "TRAMKONRIFcrouch_run_lt";
//        "TRAMKONRIFcrouch_run_rt";
//        "TRAMKONRIFcrouch_turn_lt";
//        "TRAMKONRIFcrouch_turn_rt";
//        "TRAMKONRIFidle1";
//        "TRAMKONRIFidle2croucha";
//        "TRAMKONRIFidle2crouchb";
//        "TRAMKONRIFjump_bk_idle";
//        "TRAMKONRIFjump_bk_land";
//        "TRAMKONRIFjump_bk_land_bk";
//        "TRAMKONRIFjump_bk_start";
//        "TRAMKONRIFjump_fw_idle";
//        "TRAMKONRIFjump_fw_land";
//        "TRAMKONRIFjump_fw_land_fw";
//        "TRAMKONRIFjump_fw_start";
//        "TRAMKONRIFjump_idle";
//        "TRAMKONRIFjump_land";
//        "TRAMKONRIFjump_land_fw";
//        "TRAMKONRIFjump_lt_crouch";
//        "TRAMKONRIFjump_lt_idle";
//        "TRAMKONRIFjump_lt_land";
//        "TRAMKONRIFjump_lt_land_lt";
//        "TRAMKONRIFjump_lt_start";
//        "TRAMKONRIFjump_rt_crouch";
//        "TRAMKONRIFjump_rt_idle";
//        "TRAMKONRIFjump_rt_land";
//        "TRAMKONRIFjump_rt_land_rt";
//        "TRAMKONRIFjump_rt_start";
//        "TRAMKONRIFjump_start";
//        "TRAMKONRIFkick_heavy";
//        "TRAMKONRIFpickup_cr";
//        "TRAMKONRIFpickup_mid";
//        "TRAMKONRIFpickup_rif_cr";
//        "TRAMKONRIFpickup_rif_stand";
//        "TRAMKONRIFpickup_stand";
//        "TRAMKONRIFrun1stepa";
//        "TRAMKONRIFrun1stepb";
//        "TRAMKONRIFrunstart";
//        "TRAMKONRIFrunstop";
//        "TRAMKONRIFrun_bk_1stepa";
//        "TRAMKONRIFrun_bk_1stepb";
//        "TRAMKONRIFrun_bk_lt";
//        "TRAMKONRIFrun_bk_rt";
//        "TRAMKONRIFrun_bk_ss_lt";
//        "TRAMKONRIFrun_bk_ss_rt";
//        "TRAMKONRIFrun_bk_start";
//        "TRAMKONRIFrun_bk_stop";
//        "TRAMKONRIFrun_lt";
//        "TRAMKONRIFrun_rt";
//        "TRAMKONRIFrun_slide";
//        "TRAMKONRIFrun_slide_crouch";
//        "TRAMKONRIFrun_slide_getup";
//        "TRAMKONRIFrun_slide_run";
//        "TRAMKONRIFrun_ss_lt";
//        "TRAMKONRIFrun_ss_rt";
//        "TRAMKONRIFss_lt_1stepa";
//        "TRAMKONRIFss_lt_1stepb";
//        "TRAMKONRIFss_lt_lt";
//        "TRAMKONRIFss_lt_rt";
//        "TRAMKONRIFss_lt_run";
//        "TRAMKONRIFss_lt_slide";
//        "TRAMKONRIFss_lt_start";
//        "TRAMKONRIFss_lt_stop";
//        "TRAMKONRIFss_rt_1stepa";
//        "TRAMKONRIFss_rt_1stepb";
//        "TRAMKONRIFss_rt_lt";
//        "TRAMKONRIFss_rt_rt";
//        "TRAMKONRIFss_rt_run";
//        "TRAMKONRIFss_rt_slide";
//        "TRAMKONRIFss_rt_start";
//        "TRAMKONRIFss_rt_stop";
//        "TRAMKONRIFstartle_bk1";
//        "TRAMKONRIFstartle_fw1";
//        "TRAMKONRIFstartle_lt1";
//        "TRAMKONRIFstartle_rt1";
//        "TRAMKONRIFthrow_bk_p";
//        "TRAMKONRIFthrow_bk_p_tgt";
//        "TRAMKONRIFthrow_fw_p";
//        "TRAMKONRIFthrow_fw_p_tgt";
//        "TRAMKONRIFturn_lt";
//        "TRAMKONRIFturn_rt";
//        "TRAMKONRIFwalk_back_lt";
//        "TRAMKONRIFwalk_back_rt";
//        "TRAMKONRIFwalk_back_start_lt";
//        "TRAMKONRIFwalk_back_start_rt";
//        "TRAMKONRIFwalk_back_stop_lt";
//        "TRAMKONRIFwalk_back_stop_rt";
//        "TRAMKONRIFwalk_lt";
//        "TRAMKONRIFwalk_rt";
//        "TRAMKONRIFwalk_start_lt";
//        "TRAMKONRIFwalk_start_rt";
//        "TRAMKONRIFwalk_stop_lt";
//        "TRAMKONRIFwalk_stop_rt";
//        "TRAMKONRIFwatch_idle";
//        "TRAMKONRIFwatch_start";
//        "TRAMKONRIFwatch_stop";
//        "TRAMKONSCRrun_left";
//        "TRAMKONSCRrun_right";
//        "TRAMKONSPRrun_lt";
//        "TRAMKONSPRrun_rt";
       
        */
            #endregion
            
            Oni.InstanceFileManager fm = new Oni.InstanceFileManager();
            Oni.InstanceFile level0 = fm.OpenFile((Application.isEditor ? @"D:\OniCleanInstall\" : @"..\..\") + @"GameDataFolder\level0_Final.dat");
            Oni.InstanceFile level1 = fm.OpenFile((Application.isEditor ? @"D:\OniCleanInstall\" : @"..\..\") + @"GameDataFolder\level1_Final.dat");
            
            /*:D*/
            //D:\ONI\Edition\GlobalDataFolder\ONCCTCTF_lite_2.oni
            Oni.InstanceFile ONCCKONOKO;// = fm.OpenFile(@"D:\ONI\Edition\GlobalDataFolder\ONCCKonoko_generic.oni");
            Oni.Game.CharacterClass konokoONCC = null;// = Oni.Game.CharacterClass.Read(ONCCKONOKO.GetNamedDescriptors()[0]);
            List<string> types = new List<string>();
            Oni.InstanceDescriptor level_01_ONLV = null;
            Oni.InstanceDescriptor KON_CC = null;



            {
                int couc = 0;
                
                //Debug.LogError(fm.OpenFile((Application.isEditor ? @"D:\OniCleanInstall\" : @"..\..\") + @"GameDataFolder\level0_Final.dat" + @"\BINACJBOTrigger.oni"));
                //Debug.Log(fm.FindInstance("CJBODoor", level1));
                MemoryStream ms = new MemoryStream();
                System.Xml.XmlWriter xw = System.Xml.XmlWriter.Create(ms);

                /*
                Oni.Xml.ObjcXmlExporter.Export(fm.FindInstance("BINACJBODOOR.oni", level1).Descriptors[0].OpenRead(), xw);
                xw.Flush();
                System.IO.File.WriteAllBytes("D:\\odump.xml", ms.GetBuffer());*/
                

                /*
                foreach (Oni.InstanceDescriptor desc in fm.FindInstance("hjgasdhj", level1).Descriptors)
                {
                    //Debug.LogError(desc.Name + "<P|P>" + desc.Template.Tag);
                    switch (desc.Template.Tag)
                    {
                        
                        

                        case Oni.TemplateTag.DOOR:
                            ODump(desc, true);
                            //UseDOOR(desc);
                            Debug.LogError(desc.Name + "<?|?>" + desc.Template.Tag);
                            
                            break;
                    }
                }*/

                foreach (Oni.InstanceDescriptor desc in level0.GetNamedDescriptors())
                {
                    //Debug.Log(desc.Template.Tag);
                    if (couc++ > 50)
                    {
                        couc = 0;
                        yield return null;
                    }

                    
                    switch (desc.Template.Tag)
                    {
                        case Oni.TemplateTag.DOOR:
                            UseDOOR(desc);
                            //ODump(desc);
                            break;
                        case Oni.TemplateTag.ONCC:
                            Debug.Log(desc.Name);
                            if (desc.Name == "konoko_generic")
                            {
                                Debug.Log("konoko loaded from level 0");
                                konokoONCC = Oni.Game.CharacterClass.Read(desc);
                            }
                            break;
                    }
                }

                couc = 0;


                foreach (Oni.InstanceDescriptor desc in level1.Descriptors)
                {
                    if (couc++ > 50)
                    {
                        couc = 0;
                        yield return null;
                    }
                    switch (desc.Template.Tag)
                    {
                        case Oni.TemplateTag.TXMP:
                            {
                                Texture2D[] l_tex = new Texture2D[]{ ObtainTXFrom(desc) };
                                m_textureObtainer.Add(desc.Index, () => l_tex[0]);
                            }
                            break;

                        case Oni.TemplateTag.M3GM:
                            Oni.Motoko.Geometry me = Oni.Motoko.GeometryDatReader.Read(desc);
                            //Debug.Log(desc.Index);
                            m_M3GMREG.Add(desc.Index, me);
                            break;

                        case Oni.TemplateTag.OBOA:
                            UseOBOA(desc);
                            //ODump(desc);
                            break;
                        case Oni.TemplateTag.ONOA:
                            //Debug.Log(desc.Name + ";;;;");
                            InstallONOA(desc);
                            //ODump(desc);
                            
                            break;

                       
                        case Oni.TemplateTag.OBDC:
                            
                            //Debug.Log("OBDC" + desc.Name + "|" + desc.Index);
                            break;

                        case Oni.TemplateTag.ONCC:
                            if (desc.Name == "konoko_generic" && konokoONCC == null)
                            {
                                Debug.Log("konoko is still null! daym!");
                                konokoONCC = Oni.Game.CharacterClass.Read(desc);
                                KON_CC = desc;
                            }
                            break;

                        case Oni.TemplateTag.BINA:
                            //Debug.LogError(desc.Name + "{}{}{");
                           
                            if (desc.Name.Contains("Door"))
                            {
                                InitializeDoors(desc);
                            }

                            break;
                        
                        case Oni.TemplateTag.ONLV:
                            level_01_ONLV = desc;
                            //ODump(desc, true);
                            Debug.Log(level_01_ONLV.Name);
                            break;
                    }
                }
            }

            System.DateTime dt1 = System.DateTime.Now;
            
            {
                int couc = 0;
                foreach (Oni.InstanceDescriptor de in level_01_ONLV.GetReferencedDescriptors())
                {
                    if (couc++ > 50)
                    {
                        couc = 0;
                        yield return null;
                    }
                    switch (de.Template.Tag)
                    {
                        case Oni.TemplateTag.AKEV:
                            InstallAKEV(de);
                            break;
                    }
                }
            }


            int state = 0;
            OBAN _oban = null;

            /*
            foreach (string s in T_SCRIPT.Split(" \n".ToCharArray()))
            {
                yield return null;
                //Debug.LogWarning(s);
                switch (state)
                {
                    case 0:
                        switch (s)
                        {
                            case "env_setanim": state = 1; break;
                        }
                        break;
                    case 1:
                        int __id = int.Parse(s);
                        //GameObject go = m_levelRoot.fon

                        if (m_obans.ContainsKey(__id))//TODO: __id is the animation id!
                        {
                            _oban = m_obans[__id];
                        }
                        else
                        {
                            Debug.LogWarning("anim id not found" + __id);
                        }
                        state = 2;
                        break;

                    case 2:
                        
                        Transform go = m_levelRoot.transform.FindChild(s);
                        state = 0;
                        if (go != null)
                        {
                            go.position = new Vector3(_oban.Trs.Translation.X, _oban.Trs.Translation.Y, _oban.Trs.Translation.Z);

                        }
                        else
                        {
                            Debug.Log("no such child" + s);
                        }
                        break;

                }
            }*/

            Debug.Log("stuff done in " + (System.DateTime.Now - dt1));
            IEnumerator<Oni.InstanceDescriptor> animationsEnum = konokoONCC.Animations.GetEnumerator();
            Dictionary<string, Oni.InstanceDescriptor> animactionDictionary = new Dictionary<string, Oni.InstanceDescriptor>();
            yield return null;
            for (; animationsEnum.MoveNext(); )
            {
                if (!animactionDictionary.ContainsKey("TRAM" + animationsEnum.Current.Name))
                {
                    animactionDictionary.Add("TRAM" + animationsEnum.Current.Name, animationsEnum.Current);
                }
            }

            yield return null;
            Dictionary<string, Transform> dict = new Dictionary<string, Transform>();

            #region texture placement
            List<Oni.InstanceDescriptor>.Enumerator textures = new List<Oni.InstanceDescriptor>(konokoONCC.Textures).GetEnumerator();
            textures.MoveNext();
            Oni.Totoro.Body body = Oni.Totoro.BodyDatReader.Read(@konokoONCC.Body);

            foreach (Oni.Totoro.BodyNode node in body.Nodes)
            {
                Transform t = new GameObject(node.Name).transform;

                if (node.Name == "pelvis")//TODO: get damn root node instead!
                {
                    t.parent = transform;
                }

                dict.Add(node.Name, t);
                ConstructBodypart(node.Geometry, t, Oni.Motoko.TextureDatReader.Read(textures.Current));
                textures.MoveNext();
                t.position = new Vector3(node.Translation.X, node.Translation.Y, node.Translation.Z);
                yield return null;
            }
            #endregion

            dict["pelvis"].transform.localPosition = Vector3.zero;

            Animation anim = null;
            anim = gameObject.AddComponent<Animation>();
            {
                MemoryStream ms = new MemoryStream();
                Oni.Xml.GenericXmlWriter.Write(System.Xml.XmlWriter.Create(ms), u => { }, KON_CC);
                ms.Flush();
                Debug.Log(System.Text.Encoding.UTF8.GetString(ms.GetBuffer()));
            }
            foreach (Oni.Totoro.BodyNode node in body.Nodes)
            {
                yield return null;
                ValidateChildrens(node, dict, body.Root.Name);
            }

            int _counc = 0;

            foreach (Oni.InstanceDescriptor desc in animactionDictionary.Values)
            {
                if (_counc++ > 30)
                {
                    _counc = 0;
                    yield return null;
                }
                Oni.Totoro.Animation animationReader = Oni.Totoro.AnimationDatReader.Read(desc);

                m_anims.Add(desc.Name, animationReader);
                if (animationReader.FrameSize != 6)//TODO: fix this stuff
                {
                    continue;
                }
                animationReader.ValidateFrames();
                IEnumerator<List<Oni.Totoro.KeyFrame>> boneAnimations = animationReader.Rotations.GetEnumerator();
                boneAnimations.MoveNext();
                AnimationClip clip = new AnimationClip();
                AnimationCurve curve = new AnimationCurve();
                List<List<Oni.Totoro.KeyFrame>> rotations = animationReader.Rotations;
                bool frameSize = animationReader.FrameSize == 6;
                bool flags = (int)(animationReader.Flags & Oni.Totoro.AnimationFlags.Overlay) != 0;
                uint overlayUsedBones = (uint)(animationReader.OverlayUsedBones | animationReader.OverlayReplacedBones);
                List<Oni.Totoro.BodyNode> nodes = body.Nodes;

                for (int j = 0; j < rotations.Count; j++)
                {
                    Oni.Totoro.BodyNode node = nodes[j];
                    List<Oni.Totoro.KeyFrame> keyFrames = rotations[j];
                    int count = keyFrames.Count; ;
                    float[] singleArray = new float[count];
                    float[] degrees = new float[count];
                    float[] y = new float[count];
                    float[] z = new float[count];

                    int duration = 0;
                    for (int num = 0; num < keyFrames.Count; num++)
                    {
                        Oni.Totoro.KeyFrame keyFrame = keyFrames[num];
                        singleArray[num] = (float)duration * 0.0166666675f;
                        duration = duration + keyFrame.Duration;
                        if (!frameSize)
                        {
                            Oni.Quaternion quaternion1 = new Oni.Quaternion(keyFrame.Rotation);
                            Oni.Vector3 eulerXYZ = quaternion1.ToEulerXYZ();
                            degrees[num] = Oni.MathHelper.ToDegrees(eulerXYZ.X);
                            y[num] = Oni.MathHelper.ToDegrees(eulerXYZ.Y);
                            z[num] = Oni.MathHelper.ToDegrees(eulerXYZ.Z);
                        }
                        else
                        {
                            degrees[num] = keyFrame.Rotation.X;
                            y[num] = keyFrame.Rotation.Y;
                            z[num] = keyFrame.Rotation.Z;
                        }
                    }

                    MakeRotationCurveContinuous(degrees);
                    MakeRotationCurveContinuous(y);
                    MakeRotationCurveContinuous(z);
                    List<Keyframe> kflX = new List<Keyframe>();
                    List<Keyframe> kflY = new List<Keyframe>();
                    List<Keyframe> kflZ = new List<Keyframe>();
                    List<Keyframe> kflW = new List<Keyframe>();

                    for (int _i = 0; _i < degrees.Length; _i++)
                    {
                        Oni.Quaternion qq =
                        //Oni.Quaternion.CreateFromEulerXYZ(z[_i], y[_i], degrees[_i]);
                        //Oni.Quaternion.CreateFromEulerXYZ(z[_i], degrees[_i], y[_i]);
                        //Oni.Quaternion.CreateFromEulerXYZ(y[_i], z[_i], degrees[_i]);
                        //Oni.Quaternion.CreateFromEulerXYZ(y[_i], degrees[_i], z[_i]);
                        //Oni.Quaternion.CreateFromEulerXYZ(degrees[_i], z[_i], y[_i]);
                        Oni.Quaternion.CreateFromEulerXYZ(degrees[_i], y[_i], z[_i]);
                        Quaternion val = Quaternion.Euler(0,0,0) * new Quaternion(qq.X, qq.Y, qq.Z, qq.W);
                        kflX.Add(new Keyframe(singleArray[_i], val.x));
                        kflY.Add(new Keyframe(singleArray[_i], val.y));
                        kflZ.Add(new Keyframe(singleArray[_i], val.z));
                        kflW.Add(new Keyframe(singleArray[_i], val.w));
                    }

                    clip.SetCurve(GetPath(dict[node.Name]), typeof(Transform), "m_LocalRotation.x", new AnimationCurve(kflX.ToArray()));
                    clip.SetCurve(GetPath(dict[node.Name]), typeof(Transform), "m_LocalRotation.y", new AnimationCurve(kflY.ToArray()));
                    clip.SetCurve(GetPath(dict[node.Name]), typeof(Transform), "m_LocalRotation.z", new AnimationCurve(kflZ.ToArray()));
                    clip.SetCurve(GetPath(dict[node.Name]), typeof(Transform), "m_LocalRotation.w", new AnimationCurve(kflW.ToArray()));
                }

                List<float> posXList = new List<float>();
                List<float> posYList = new List<float>();
                List<float> posZList = new List<float>();

                for (int k = 0; k < animationReader.Velocities.Count; k++)
                {
                    posXList.Add(animationReader.Velocities[k].X);
                    if (animationReader.Heights.Count <= k)
                    {
                        posYList.Add(0);
                    }
                    else
                    {
                        posYList.Add(animationReader.Heights[k]);
                    }
                    posZList.Add(animationReader.Velocities[k].Y);
                }

                {
                    float _iien = 0;
                    float timer = 0;
                    clip.SetCurve("", typeof(GUIANIMCONTROL), "m_motionVector.x", new AnimationCurve(posXList.ConvertAll<Keyframe>(frame =>
                    {
                        Keyframe res = new Keyframe(timer += 0.0166666675f, frame / 0.0166666675f);
                        return res;
                    }).ToArray()));
                }

                {
                    float _iien = 0;
                    float timer = 0;
                    clip.SetCurve(GetPath(dict["pelvis"]), typeof(Transform), "m_LocalPosition.y", new AnimationCurve(posYList.ConvertAll<Keyframe>(frame =>
                    {
                        Keyframe res = new Keyframe(timer += 0.0166666675f, frame);
                        _iien = frame;
                        return res;
                    }).ToArray()));
                }

                {
                    float _iien = 0;
                    float timer = 0;
                    clip.SetCurve("", typeof(GUIANIMCONTROL), "m_motionVector.z", new AnimationCurve(posZList.ConvertAll<Keyframe>(frame =>
                    {
                        Keyframe res = new Keyframe(timer += 0.0166666675f, frame / 0.0166666675f);
                        return res;
                    }).ToArray()));
                    AnimationEvent ev = null;
                    clip.AddEvent(ev = new AnimationEvent() { objectReferenceParameter = GetComponent<GUIANIMCONTROL>(), functionName = "OnActionFrame", time = timer - 1 / 60f, stringParameter = desc.Name });
                    m_events.Add(desc.Name, ev);
                }

                clip.wrapMode = WrapMode.ClampForever;
                clip.EnsureQuaternionContinuity();
                anim.AddClip(clip, desc.Name);
                clip.name = desc.Name;
                GUIANIMCONTROL.m_clips.Add(clip);
            }


            sr = gameObject.AddComponent<SkinnedMeshRenderer>();
            Mesh m = new Mesh();
            m.vertices = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right, Vector3.up + Vector3.right };
            m.triangles = new int[] { 0, 1, 2, 3, 2, 1 };
            m.boneWeights = new BoneWeight[4] 
        { 
            new BoneWeight() { boneIndex0 = 0, weight0 = 1 },
            new BoneWeight() { boneIndex0 = 0, weight0 = 1 },
            new BoneWeight() { boneIndex0 = 0, weight0 = 1 },
            new BoneWeight() { boneIndex0 = 0, weight0 = 1 },
        };

            m.bindposes = new Matrix4x4[] 
        {
            Matrix4x4.identity,
        };

            sr.bones = new Transform[] { new GameObject().transform };
            sr.sharedMesh = m;
            EndFlag = true;
        }
    }

    void InitializeDoors(Oni.InstanceDescriptor desc)
    {
        MemoryStream ms = ODumpBINA(desc);
        XmlRootAttribute xRoot = new XmlRootAttribute();
        xRoot.ElementName = "Objects";
        xRoot.IsNullable = true;
        Round2.BINADOOR[] l_doors = new System.Xml.Serialization.XmlSerializer(typeof(Round2.BINADOOR[]), xRoot).Deserialize(ms) as Round2.BINADOOR[];
        Debug.Log(l_doors[0].Id);

        foreach (Round2.BINADOOR l_door in l_doors)
        {
            OBOA_Instantiator.InitializeFrom(l_door);
        }
    }

    private MemoryStream ODumpBINA(Oni.InstanceDescriptor desc, bool p = false)
    {
        MemoryStream l_ms = new MemoryStream();
        System.Xml.XmlWriter l_xmlw = System.Xml.XmlWriter.Create(l_ms);
        int num;
        Oni.BinaryReader binaryReader = desc.OpenRead();
        using (binaryReader)
        {
            binaryReader.ReadInt32();
            num = binaryReader.ReadInt32();
        }
        Oni.BinaryReader rawReader = desc.GetRawReader(num);
        using (rawReader)
        {
            Oni.Metadata.BinaryTag binaryTag = (Oni.Metadata.BinaryTag)rawReader.ReadInt32();
            Oni.Metadata.BinaryTag binaryTag1 = binaryTag;
            if (binaryTag1 > Oni.Metadata.BinaryTag.ONIE)
            {
                if (binaryTag1 == Oni.Metadata.BinaryTag.PAR3)
                {
                    ParticleXmlExporter.Export(desc.FullName.Substring(8), rawReader, l_xmlw);
                }
                else
                {
                    if (binaryTag1 == Oni.Metadata.BinaryTag.SABD)
                    {
                        Oni.Sound.SabdXmlExporter.Export(rawReader, l_xmlw);
                    }
                    else
                    {
                        if (binaryTag1 != BinaryTag.TMBD)
                        {
                            throw new System.NotSupportedException(string.Format("Unsupported BINA type '{0}'", Oni.Utils.TagToString((int)binaryTag)));
                        }
                        TmbdXmlExporter.Export(rawReader, l_xmlw);
                    }
                }
            }
            else
            {
                if (binaryTag1 == BinaryTag.OBJC)
                {
                    ObjcXmlExporter.Export(rawReader, l_xmlw);
                }
                else
                {
                    if (binaryTag1 != BinaryTag.ONIE)
                    {
                        throw new System.NotSupportedException(string.Format("Unsupported BINA type '{0}'", Oni.Utils.TagToString((int)binaryTag)));
                    }
                    OnieXmlExporter.Export(rawReader, l_xmlw);
                }
            }
        }
        l_xmlw.Flush();
        if (p)
        {
            System.IO.File.WriteAllBytes("D:\\odump.xml", l_ms.GetBuffer());
        }
        l_ms.Seek(0, SeekOrigin.Begin);
        return l_ms;
    }

    static Dictionary<int, OBOAObject> m_IDCtoOBOA = new Dictionary<int, OBOAObject>();
    static Dictionary<int, Oni.Motoko.Geometry> m_M3GMREG = new Dictionary<int, Oni.Motoko.Geometry>();

    /// <summary>
    /// Install OBOA's. Doors are also OBOA's
    /// </summary>
    /// <param name="desc">descriptor of installatee</param>
    private void UseOBOA(Oni.InstanceDescriptor desc)
    {
        MemoryStream ms = ODump(desc);
        ms.Seek(0, SeekOrigin.Begin);
        //OBOA o = new System.Xml.Serialization.XmlSerializer(typeof(OBOA)).Deserialize(ms) as OBOA;

        Round2.OBOA l_oboa = new System.Xml.Serialization.XmlSerializer(typeof(Round2.OBOA)).Deserialize(ms) as Round2.OBOA;
        
        /*
        MemoryStream ms2 = new MemoryStream();
        new System.Xml.Serialization.XmlSerializer(typeof(Round2.OBOA)).Serialize(ms2, l_oboa);
        Debug.LogError(new string( new List<byte>( ms2.GetBuffer()).ConvertAll<char>( u => (char)u).ToArray()) );
        //Debug.LogWarning(l_oboa.Objects[0]._Position);
        */
        return;
        System.Xml.XmlReader reader = System.Xml.XmlReader.Create(ms);
        Debug.Log(reader.Read());
        Debug.Log(reader.NodeType);
        reader.Read();
        Debug.Log(reader.NodeType);
        System.Xml.XmlReader subreader = reader.ReadSubtree();
        
        System.Xml.Linq.XDocument doc = null;
        doc = System.Xml.Linq.XDocument.Load(reader);
        
        foreach (System.Xml.Linq.XElement node in doc.Elements())
        {
            foreach (System.Xml.Linq.XElement objects in node.Elements())
            {
                foreach (System.Xml.Linq.XElement OBOAObject in objects.Elements())
                {
                    IEnumerable<System.Xml.Linq.XElement> els = OBOAObject.Elements();
                    IEnumerator<System.Xml.Linq.XElement> els_e = els.GetEnumerator();
                    els_e.MoveNext();
                    System.Xml.Linq.XElement geometry = els_e.Current;
                    GameObject usedObject = new GameObject();
                    els_e.MoveNext();
                    System.Xml.Linq.XElement anim = els_e.Current;
                    els_e.MoveNext();
                    els_e.MoveNext();
                    els_e.MoveNext();
                    els_e.MoveNext();
                    els_e.MoveNext();
                    els_e.MoveNext();
                    els_e.MoveNext();
                    string[] pos = els_e.Current.Value.Split(" ".ToCharArray());
                    usedObject.transform.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                    els_e.MoveNext();
                    string[] rot = els_e.Current.Value.Split(" ".ToCharArray());
                    //Debug.Log(els_e.Current.Value);
                    Oni.Quaternion q = new Oni.Quaternion(float.Parse(rot[0]), float.Parse(rot[1]), float.Parse(rot[2]), float.Parse(rot[3]));
                    Oni.Vector3 v = q.ToEulerXYZ();
                    usedObject.transform.rotation = Quaternion.Euler(v.Y, v.X, v.Z);
                    els_e.MoveNext();
                    string scale = els_e.Current.Value;
                    usedObject.transform.localScale = Vector3.one * float.Parse(scale);
                    els_e.MoveNext();
                    string[] _transform = els_e.Current.Value.Split(" ".ToCharArray());
                    Matrix4x4 t = new Matrix4x4();

                    //Debug.Log(anim);

                    if (geometry.HasElements)
                    {
                        IEnumerator<System.Xml.Linq.XElement> m3ga_s = geometry.Elements().GetEnumerator();
                        m3ga_s.MoveNext();
                        System.Xml.Linq.XElement m3ga = m3ga_s.Current;
                        int m3gaid = int.Parse(m3ga.FirstAttribute.Value);
                        usedObject.name = "M3GA:" + m3gaid;
                        System.Xml.Linq.XElement m3gaGeoColliection = new LinkedList<System.Xml.Linq.XElement>(m3ga.Elements()).First.Value;
                        
                        foreach (System.Xml.Linq.XElement m3ga_el in m3gaGeoColliection.Elements())
                        {
                            System.Xml.Linq.XElement m3gm_el = new LinkedList<System.Xml.Linq.XElement>(m3ga_el.Elements()).First.Value;
                            List<System.Xml.Linq.XElement> m3ga_fields = new List<System.Xml.Linq.XElement>(m3gm_el.Elements());
                            int m3gmid = int.Parse(m3gm_el.FirstAttribute.Value);
                            GameObject innerObject = new GameObject("M3GM:" + m3gmid);
                            innerObject.transform.parent = usedObject.transform;
                            innerObject.transform.localPosition = Vector3.zero;
                            innerObject.transform.localRotation = Quaternion.identity;
                            Debug.Log(m3gmid);
                            Debug.Log(m_M3GMREG.ContainsKey(m3gmid));

                            if (m_M3GMREG.ContainsKey(m3gmid) && m_M3GMREG[m3gmid].Texture != null)
                            {
                                ConstructBodypart(m_M3GMREG[m3gmid], innerObject.transform, Oni.Motoko.TextureDatReader.Read(m_M3GMREG[m3gmid].Texture));
                            }

                            //m_

                            //Debug.Log(innerObject.transform.parent.ToString(), innerObject.transform.parent);
                        }

                        //MeshFilter usedMeshFilter = 
                    }

                    if (anim.HasElements)
                    {
                        //Debug.Log("ANIM ELEMS");
                    }
                }
            }
        }
        /*
        while (subreader.Read())
        {
            switch (subreader.Name)
            {
                case "OBOAObject":
                    
                    break;
                case "M3GA": 
                    if(subreader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        subreader.ReadAttributeValue();
                        M3GA_id = int.Parse(subreader.GetAttribute(0));
                    }
                    break;
                case "Geometry":
                    if(subreader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        usedObject = new GameObject();
                        usedMeshFilter = usedObject.AddComponent<MeshFilter>();
                    }
                    break;
                case "Name":
                    if (subreader.NodeType != System.Xml.XmlNodeType.EndElement)
                    {
                        Debug.Log(subreader.HasValue + "|" + subreader.LocalName + "::SNAME");
                    }
                    break;
                case "Link":
                case "OBOA":
                case "objects": break;
                default: break;
            }
        }*/
    }

    Vector3[] UsePNTA(XElement pnta)
    {
        return null;
    }

    void UseDOOR(Oni.InstanceDescriptor desc)
    {
        MemoryStream ms = ODump(desc, true);
        ms.Seek(0, SeekOrigin.Begin);
        //OBOA o = new System.Xml.Serialization.XmlSerializer(typeof(OBOA)).Deserialize(ms) as OBOA;

        Round2.DOOR l_DOOR = new System.Xml.Serialization.XmlSerializer(typeof(Round2.DOOR)).Deserialize(ms) as Round2.DOOR;
        Debug.Log("|::|>>" +l_DOOR.id + ":doorname | " + desc.Name);
        Round2.DOOR.m_doorClasses.Add(desc.Name, l_DOOR);
    }

    int[] UseIDXA(XElement idxa)
    {
        return null;
    }

    Vector2[] UseTXCA(XElement txca)
    {
        return null;
    }

    /// <summary>
    /// Adds ONOA to ONOA registry; TODO: clean this shit up and out! Must get rid of ONOA registry, use proper XML-through AKEV loading 
    /// </summary>
    /// <param name="id">id</param>
    private void InstallONOA(Oni.InstanceDescriptor id)
    {
        MemoryStream ms = ODump(id);
        ms.Seek(0, SeekOrigin.Begin);

        foreach (ONOAElement el in (new System.Xml.Serialization.XmlSerializer(typeof(ONOA)).Deserialize(ms) as ONOA).Elements)
        {
            el._RealObjectId = el.RealObjectId;
            Debug.LogWarning("=====" + el.RealObjectId);

            if (el.RealObjectId != -1 && !m_objectONOAs.ContainsKey(el.RealObjectId))
            {
                m_objectONOAs.Add(el.RealObjectId, el);
                //onoalist.Add(el);
            }
        }
        //throw new System.NotImplementedException();
    }

    internal static Dictionary<string, Oni.Totoro.Animation> m_anims = new Dictionary<string, Oni.Totoro.Animation>();
    bool excflag = false;
    string exceptionstring = "";
    public static Dictionary<string, AnimationEvent> m_events = new Dictionary<string, AnimationEvent>();

    void OnGUI()
    {
        if (excflag)
        {
            if (!EndFlag)
            {
                GUILayout.Label("Error occured : \n" + exceptionstring);
            }
        }
        else
        {
            if (!EndFlag)
            {
                GUILayout.Label("Loading" + (Mathf.Sin(Time.time * 4) > 0 ? ".." : "."));
            }
        }

        
    }

    public static bool EndFlag = false;

	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerHit(Collider other)
    {
        Debug.LogError("trig", other);
        MonoBehaviour l_mb = other.GetComponent<MonoBehaviour>();

        if (l_mb is IOnTriggerHit)
        {
            (l_mb as IOnTriggerHit).OnHit();
        }
    }

    public void OnDoorInteract(OBOA_Instantiator door)
    {
        door.m_onTriggerHit();
    }
}
