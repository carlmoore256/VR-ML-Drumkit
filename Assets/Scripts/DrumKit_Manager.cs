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
    public bool saveSetup;
    public bool loadSetup;

    public GameObject[] drumPrefabs;

    void Start()
    {
        LoadSetup();
    }

    private void Update()
    {
        if(saveSetup)
        {
            print("SAVING SETUP FROM INSPECTOR!");
            SaveSetup();
            saveSetup = false;
        }

        if (loadSetup)
        {
            print("LOADING SETUP FROM INSPECTOR!");
            LoadSetup();
            loadSetup = false;
        }
    }

    public void SaveSetup()
    {
        //Transform[] drumComponents = GetComponentsInChildren<Transform>();

        string filename = "Assets/Data/drum_save.xml";

        TextWriter writer = new StreamWriter(filename);

        foreach (GameObject drum in drumPrefabs)
        {
            DrumInfo drumInfo = new DrumInfo(drum.transform);
            XmlSerializer serializer = new XmlSerializer(typeof(DrumInfo));
            serializer.Serialize(writer, drumInfo);
            
        }
        writer.Close();
    }

    public void LoadSetup()
    {
        print("LOADING DRUM INTO DRUM INFO");

        string filename = "Assets/Data/drum_save.xml";

        XmlSerializer serializer = new XmlSerializer(typeof(DrumInfo));
        TextReader reader = new StreamReader(filename);
        DrumInfo drumInfo = serializer.Deserialize(reader) as DrumInfo;
        print(drumInfo);
        GameObject newDrum = Instantiate(GetPrefabByName(drumInfo.prefabName), transform);
        newDrum.transform.position = drumInfo.position;

    }

    void RemoveExistingModels()
    {

    }

    // get a name match for the prefab when instantiating
    GameObject GetPrefabByName(string name)
    {
        foreach (GameObject g in drumPrefabs)
            if (g.name == name)
                return g;
        return null;
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
