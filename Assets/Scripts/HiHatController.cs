using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiHatController : MonoBehaviour
{
    public GameObject cymbalTop;
    public GameObject cymbalBottom;

    public float bottomMovementRange = 0.025f;

    Vector3 bottomStartPos;

    Rigidbody cymbalTopRb;
    Rigidbody cymbalBottomRb;

    float m_PedalValue = 1f;

    NoteCallback ncb;

    MoveableProp mp;

    private void Start()
    {
        mp = GetComponent<MoveableProp>();
        ncb = GameObject.Find("Manager").GetComponent<NoteCallback>();
        cymbalTopRb = cymbalTop.GetComponent<Rigidbody>();
        cymbalBottomRb = cymbalBottom.GetComponent<Rigidbody>();
        bottomStartPos = cymbalBottom.transform.position;
    }

    private void Update()
    {
        if (!mp.m_Repositioning)
        {
            m_PedalValue = ncb.m_HiHatPedal;
            Vector3 bottomOffset = m_PedalValue * new Vector3(0, bottomMovementRange, 0);
            cymbalBottom.transform.position = bottomStartPos + bottomOffset;
        } else
        {
            bottomStartPos = cymbalBottom.transform.position;
        }
    }
}
