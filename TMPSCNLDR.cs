using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TMPSCNLDR : MonoBehaviour 
{
    static T DeserializeAs<T>(System.IO.MemoryStream ms) where T : new()
    {
        return (T)new System.Xml.Serialization.XmlSerializer(typeof(T)).Deserialize(ms);
    }

    [Serializable]
    public struct TMP_TX_ENTRY
    {
        public string name;
        public Texture2D tex;
    }

    public List<string> m_texNames = new List<string>();
    public static TMPSCNLDR m_singleton;
    public Texture2D m_targeted_query_texture;
    public List<Mesh> m_meshLst = new List<Mesh>();

    public List<TMP_TX_ENTRY> m_txs = new List<TMP_TX_ENTRY>();

    static void UseONLV(Oni.InstanceDescriptor ides)
    {
        Round2.ONLV l_onlv;
        Debug.Log((l_onlv = DeserializeAs<Round2.ONLV>(ides.AsXmlStream())).Environment.AKEV.Quads.AGQG.Quads[0].Points.Length);
        l_onlv.InitEnv();
    }

    static void UseAKEV(Oni.InstanceDescriptor ides)
    {
        ides.AsXmlStream();
    }

    static void UseDOOR(Oni.InstanceDescriptor ides)
    {
        Round2.DOOR des = DeserializeAs<Round2.DOOR>(ides.AsXmlStream());
        Debug.Log(des.id);
    }

    static Dictionary<Oni.TemplateTag, Action<Oni.InstanceDescriptor>> m_installers = new Dictionary<Oni.TemplateTag, Action<Oni.InstanceDescriptor>>
    {
         { Oni.TemplateTag.ONLV, UseONLV },
         //{ Oni.TemplateTag.AKEV, UseAKEV },
         //{ Oni.TemplateTag.DOOR, UseDOOR },
    };

    internal Texture2D ObtainTXFrom(Oni.InstanceDescriptor txca)
    {
        //Debug.Log(txca.Index);
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
                //Debug.Log(l_t +"|"+txca.Index, l_t);
                return l_t;
            }
            catch (System.Exception ee)
            {
                Debug.LogError("TXCA id : " + txca.Index + "\nName:" + txca.Name + "\n" + ee.ToString());
                return null;
            }
        }
    }

	// Use this for initialization
	void Start () 
    {
        //Debug.Log((new System.Object[] { 113, 123, "asd" }).ArrayAsSml());
        //Debug.Log((new System.Object[] { 113, 123, "asd", Round2.GunkFlags.Danger }).ArrayAsSml().SmlToArray());
        //return;
        m_singleton = this;
        Oni.InstanceFileManager fm = new Oni.InstanceFileManager();
        Oni.InstanceFile level0 = fm.OpenFile((Application.isEditor ? @"D:\OniCleanInstall\" : @"..\..\") + @"GameDataFolder\level0_Final.dat");
        Oni.InstanceFile level1 = fm.OpenFile((Application.isEditor ? @"D:\OniCleanInstall\" : @"..\..\") + @"GameDataFolder\level1_Final.dat");
        
        foreach (Oni.InstanceDescriptor desc in level1.GetNamedDescriptors())
        {
            if (m_installers.ContainsKey(desc.Template.Tag))
            {
                m_installers[desc.Template.Tag](desc);
            }
        }

        List<int> l_txIndArray = new List<int>(Texture2DQuery.TexturesToLoad);


        foreach (Oni.InstanceDescriptor desc in level1.Descriptors)
        {
            switch (desc.Template.Tag)
            {
                case Oni.TemplateTag.TXMP:
                    {
                        //Debug.Log("idxt:" + desc.Index);
                        if (l_txIndArray.Contains(desc.Index))
                        {
                            Texture2D l_tex = null;

                            if ((l_tex = ObtainTXFrom(desc)) != null)
                            {
                                Texture2DQuery.Loaded(desc.Index, l_tex);
                                //Debug.Log(l_tex, l_tex);
                            }
                        }
                    }
                    break;
            }
        }

        Round2.AISACharacter.GetByName("Konoko")[0].TMP_InstallNewCharacter();

        /*
        foreach (int id in Texture2DQuery.TexturesToLoad)
        {
            Debug.Log(id);
            Oni.InstanceDescriptor l_ides = level0.GetDescriptor(id);
            Texture2D l_tex = null;

            if (l_ides != null)
            {
                Debug.Log(l_ides.Index == id);

                if (l_ides != null && (l_tex = ObtainTXFrom(l_ides)) != null)
                {
                    Texture2DQuery.Loaded(id, l_tex);
                    Debug.Log(l_tex, l_tex);
                }
            }
        }*/
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
