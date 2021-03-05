using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick_Manager : MonoBehaviour
{
    public GameObject stick_pf;
    public Transform handL;
    public Transform handR;

    private GameObject stickL;
    private GameObject stickR;

    private bool parentFlag;

    void Start()
    {
        print("SPAWNING STICKS");
        stickL = Instantiate(stick_pf, handL.position, Quaternion.identity);
        stickR = Instantiate(stick_pf, handR.position, Quaternion.identity);
        parentFlag = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (parentFlag)
        {
            stickL.transform.parent = handL;
            stickR.transform.parent = handR;
            parentFlag = false;
        }

    }
}
