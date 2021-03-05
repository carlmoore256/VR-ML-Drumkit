using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
public class DrumInfo
{
    public string prefabName;
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;

    public DrumInfo() { }

    public DrumInfo(Transform drum)
    {
        prefabName = drum.name.Replace(" (Clone)", string.Empty);
        position = drum.position;
        scale = drum.localScale;
        rotation = drum.rotation;
    }
}

public class DrumKit_Manager : MonoBehaviour
{
    void Start()
    {
        
    }

    void SaveSetup()
    {
        GameObject[] drumComponents = GetComponentsInChildren<GameObject>();
        string filename = "drum_save.xml";

        TextWriter writer = new StreamWriter(filename);

        foreach (GameObject drum in drumComponents)
        {
            DrumInfo drumInfo = new DrumInfo(drum.transform);
            XmlSerializer serializer = new XmlSerializer(typeof(DrumInfo));
            serializer.Serialize(writer, drumInfo);
            
        }
        writer.Close();
    }

    // rootobject is the prefab
    void Load(GameObject rootObject, string filename)
    {
        //XmlSerializer serializer = new XmlSerializer(typeof(BuildingInfo));
        //TextReader reader = new StreamReader(filename);
        //DrumInfo drumInfo = serializer.Deserialize(reader) as BuildingInfo;
        //drumInfo.Instantiate(rootObject);
    }

}
