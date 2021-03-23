using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteractions : MonoBehaviour
{
    public GameObject mSelectionSphere;
    private OVRInput.Controller mController;
    private MoveableProp mSelectedProp;

    private bool mGrabFlag;

    void Start()
    {
        //mSelectionSphere = Instantiate(selectionSpherePf, this.transform);
        mSelectionSphere.SetActive(false);

        mSelectedProp = null;

        if(transform.name == "LeftHandAnchor"){
            mController = OVRInput.Controller.LTouch;
        } else if(transform.name =="RightHandAnchor")
        {
            mController = OVRInput.Controller.RTouch;
        }
        mSelectedProp = null;
        mGrabFlag = true;
    }

    void Update()
    {
        if(checkMoveableAction())
        {
            mSelectionSphere.SetActive(true);
            mSelectionSphere.transform.position = transform.position;

            if(mSelectedProp != null)
            {
                mSelectedProp.MoveProp(transform.position, mGrabFlag);
                Vector2 scale = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                mSelectedProp.ScaleProp(scale.y * 0.5f);

                if (mGrabFlag)
                    mGrabFlag = false;
            }
        } else {
            mSelectedProp = null;
            mSelectionSphere.SetActive(false);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if(checkMoveableAction() && mSelectedProp == null && collider.GetComponent<MoveableProp>() != null)
        {
            mGrabFlag = true;
            mSelectedProp = collider.GetComponent<MoveableProp>();
            mSelectedProp.SelectionHover();
        }
    }

    bool checkMoveableAction()
    {
        return (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, mController));
    }

    IEnumerator sizeLerp(GameObject g, float scalar)
    {
        yield return new WaitForSeconds(0.5f);

    }
}
