using UnityEngine;
using System.Collections;
using System;
using System.IO;

public static class Extensions  
{
    internal static MemoryStream AsXmlStream(this Oni.InstanceDescriptor des)
    {
        MemoryStream l_ms = new MemoryStream();
        System.Xml.XmlWriter l_w = System.Xml.XmlWriter.Create(l_ms);
        Oni.Action<Oni.InstanceDescriptor> l_ides = null;
        l_ides = u =>
        {
            try
            {
                Oni.Xml.GenericXmlWriter.Write(l_w, l_ides, u);
            }
            catch (System.Exception ee)
            {
                //Debug.Log(ee);
                //Debug.Log(des.IsPlaceholder);
            }
        };
        Oni.Xml.GenericXmlWriter.Write(l_w, l_ides, des);
        l_w.Flush();
        l_ms.Seek(0, SeekOrigin.Begin);
        //Debug.Log(System.Text.UTF8Encoding.UTF8.GetString(l_ms.GetBuffer()));
        //System.IO.File.WriteAllBytes("D:\\odump.xml", l_ms.GetBuffer());
        //Debug.Log("written do disk");
        return l_ms;
    }
}
