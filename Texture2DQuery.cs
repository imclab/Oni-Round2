using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Texture2DQuery
{
    public static Dictionary<int, List<System.Action<Texture2D>>> m_queries = new Dictionary<int, List<System.Action<Texture2D>>>();

    public static void TexturePend(int texId, System.Action<Texture2D> ask)
    {
        if (!m_queries.ContainsKey(texId))
        {
            m_queries.Add(texId, new List<System.Action<Texture2D>>());
        }

        m_queries[texId].Add(ask);
    }

    public static IEnumerable<int> TexturesToLoad
    {
        get
        {
            return m_queries.Keys;
        }
    }

    public static void Loaded(int id, Texture2D tex)
    {
        if (m_queries.ContainsKey(id))
        {
            foreach (System.Action<Texture2D> act in m_queries[id])
            {
                try
                {
                    act(tex);
                }
                catch (System.Exception ee)
                {
                    Debug.LogWarning(ee.ToString());
                }
            }
        }
    }
}
