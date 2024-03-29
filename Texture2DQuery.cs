﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Texture2DQuery
{
    public static Dictionary<int, Texture2D> m_idToTex = new Dictionary<int, Texture2D>();
    public static Dictionary<int, List<System.Action<Texture2D>>> m_queries = new Dictionary<int, List<System.Action<Texture2D>>>();

    public static void TexturePend(int texId, System.Action<Texture2D> ask)
    {
        if(m_idToTex.ContainsKey(texId))
        {
            ask(m_idToTex[texId]);
        }
        else
        {
            if (!m_queries.ContainsKey(texId))
            {
                m_queries.Add(texId, new List<System.Action<Texture2D>>());
            }

            m_queries[texId].Add(ask);
        }
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
        m_idToTex.Add(id, tex);

        if (m_queries.ContainsKey(id))
        {
            foreach (System.Action<Texture2D> act in m_queries[id])
            {
                try
                {
                    act(tex);

                    if (!TMPSCNLDR.m_singleton.m_texNames.Contains(tex.name + "|" + id.ToString()))
                    {
                        TMPSCNLDR.m_singleton.m_texNames.Add(tex.name + "|" + id.ToString());
                        TMPSCNLDR.m_singleton.m_txs.Add(new TMPSCNLDR.TMP_TX_ENTRY() { name = tex.name + "|" + id.ToString(), tex = tex });
                    }
                }
                catch (System.Exception ee)
                {
                    Debug.LogWarning(ee.ToString());
                }
            }
        }
    }
}
