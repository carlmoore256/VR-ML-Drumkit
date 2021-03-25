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

public class AllDrumInfo
{
    [XmlElement("DrumInfo")]
    public List<DrumInfo> drumInfoList = new List<DrumInfo>();
}

public class DrumKit_Manager : MonoBehaviour
{
    // loads default drum configuration on start
    public bool loadConfigOnStart = true;

    public bool saveSetup;
    public bool loadSetup;

    public GameObject[] drumPrefabs;

    void Start()
    {
        if (loadConfigOnStart)
            LoadSetup("Assets/Data/drum_config_default.xml");
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

    public void SaveSetup(string filename = "Assets/Data/drum_save.xml")
    {
        TextWriter writer = new StreamWriter(filename);
        AllDrumInfo allDrumInfo = new AllDrumInfo();

        foreach (GameObject drum in GetActiveDrums())
        {
            DrumInfo drumInfo = new DrumInfo(drum.transform);
            allDrumInfo.drumInfoList.Add(drumInfo);
        }
        XmlSerializer serializer = new XmlSerializer(typeof(AllDrumInfo));
        serializer.Serialize(writer, allDrumInfo);

        writer.Close();
    }

    public void LoadSetup(string filename = "Assets/Data/drum_save.xml")
    {
        print("LOADING DRUM INTO DRUM INFO");
        RemoveExistingModels();

        XmlSerializer serializer = new XmlSerializer(typeof(AllDrumInfo));
        TextReader reader = new StreamReader(filename);
        AllDrumInfo allDrumInfo = serializer.Deserialize(reader) as AllDrumInfo;

        foreach(DrumInfo di in allDrumInfo.drumInfoList)
        {
            print(di.position);
            GameObject newDrum = Instantiate(GetPrefabByName(di.prefabName), transform);
            newDrum.transform.position = di.position;
            newDrum.transform.rotation = di.rotation;
            newDrum.transform.localScale = di.scale;
            newDrum.name = newDrum.name.Replace("(Clone)", "");
        }
    }

    // removes drums for loading new config
    void RemoveExistingModels()
    {
        print("removing existing models");
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach(Transform t in allChildren)
        {
            if (t != transform) // apparently [this] is in allChildren
                Destroy(t.gameObject);
        }
    }

    // get a name match for the prefab when instantiating
    GameObject GetPrefabByName(string name)
    {
        foreach (GameObject g in drumPrefabs)
            if (g.name == name)
                return g;
        return null;
    }

    // return a list of active gameobjects that are drums
    GameObject[] GetActiveDrums()
    {
        List<GameObject> activeDrums = new List<GameObject>();
        foreach (GameObject g in drumPrefabs)
        {
            //print(g.name);
            //print(transform.Find(g.name))
            activeDrums.Add(transform.Find(g.name).gameObject);

        }
        return activeDrums.ToArray();
    }
}
