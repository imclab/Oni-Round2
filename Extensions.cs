using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public static class Extensions  
{
    const string SMLSIG = "[ SML :: ";

    internal static object[] SmlToArray(this string data)
    {
        if (data.StartsWith(SMLSIG))
        {
            string[] l_vals = data.Split('|');
            List<object> l_res = new List<object>();

            for (int i = 1; i < l_vals.Length;i++)
            {
                string[] l_contents = l_vals[i].Split('~');
                string l_typename = l_contents[0];
                string l_val = l_contents[1].Remove(0,1);
                int l_rNumber = l_val[l_val.Length - 1] == '}' ? 1 : 2;
                l_val = l_val.Remove(l_val.Length - l_rNumber, l_rNumber);
                //Debug.Log(l_typename);
                System.Type l_type = System.Type.GetType(l_typename);
                System.Object l_resval = null;

                if (l_val.Length > 0 && l_type != null && (l_type.IsAssignableFrom(typeof(IConvertible)) || l_type.IsPrimitive || l_type == typeof(string) || l_type.IsEnum))
                {
                    if (l_type.IsEnum)
                    {
                        l_resval = Enum.Parse(l_type, l_val, true);
                    }
                    else
                    {
                        l_resval = Convert.ChangeType(l_val, l_type);
                    }
                }

                l_res.Add(l_resval);
                //Debug.Log(l_resval);
            }

            return l_res.ToArray();
        }
        else
        {
            Debug.LogError("sml sig not detected");
            return null;
        }
    }
   
    internal static string ArrayAsSml<T>(this T[] data)
    {
        string l_s = SMLSIG;

        foreach (T member in data)
        {
            l_s += "|" + member.GetType().AssemblyQualifiedName + "~{" + member.ToString() + "}";
        }

        return l_s + "]";
    }

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
        return l_ms;
    }
}
