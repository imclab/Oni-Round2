using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Light))]
public class LightInspector : Editor 
{
	
	// Update is called once per frame
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        if (EditorGUILayout.Toggle("Save",false))
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Light" + base.target.GetInstanceID() + ".txt");
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            System.IO.StreamWriter s;
            System.Xml.Serialization.XmlSerializer.FromTypes(new System.Type[]{typeof(NewBehaviourScript.LightData)})[0].Serialize(System.Xml.XmlWriter.Create( s= System.IO.File.AppendText(path)), new NewBehaviourScript.LightData(target as Light));
            s.Flush();
            s.Close();
        }
    }
}
