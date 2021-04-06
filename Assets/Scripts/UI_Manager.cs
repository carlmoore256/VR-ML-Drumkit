﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{

    public MotionPreview mp_l;
    public MotionPreview mp_r;

    public Material selectionMaterial;

    public CaptureMotion captureMotion;

    public GameObject UI_panel;

    public GameObject linePointer_L;
    public GameObject linePointer_R;
    public Transform stick_L;
    public Transform stick_R;

    public LayerMask layerMask;

    public OVRPlayerController playerController;

    public Button btn;

    private Image img;

    private LineRenderer lr_l;
    private LineRenderer lr_r;

    private Vector3 linePointerDist = new Vector3(0, 0, 20);

    bool playerMovement = true; // begin with movement enabled
    bool capToggle;
    bool stickAdjust;

    public DrumKit_Manager dkMan;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();

        UI_panel.SetActive(false);

        // initialize line pointers for when menu pops up
        lr_l = linePointer_L.GetComponent<LineRenderer>();
        lr_r = linePointer_R.GetComponent<LineRenderer>();

        SetLinePointersActive(false);
    }

    void Update()
    {
        CheckMenuToggle();

        if (UI_panel.activeSelf)
        {
            if (RayCastMenu(linePointer_L, lr_l) && OVRInput.GetUp(OVRInput.Button.Three))
                btn.onClick.Invoke();

            if (RayCastMenu(linePointer_R, lr_r) && OVRInput.GetUp(OVRInput.Button.One))
                btn.onClick.Invoke();

            if (stickAdjust)
            {
                AdjustStickPosition(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick), stick_L);
                AdjustStickPosition(OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick), stick_R);
            }
        }
    }

    void AdjustStickPosition(Vector2 thumbStick, Transform drumStick)
    {
        drumStick.Translate(Vector3.forward * thumbStick.y * 0.01f);
        drumStick.Rotate(new Vector3(thumbStick.x * 0.01f, 0, 0));
    }

    bool RayCastMenu(GameObject linePointer, LineRenderer lr)
    {
        bool hitBtn = false;

        Vector3[] points = new Vector3[2];
        points[0] = linePointer.transform.position;

        Ray ray;

        ray = new Ray(linePointer.transform.position, linePointer.transform.parent.forward);
        //ray = new Ray(linePointer.transform.position, linePointer.transform.forward);
        RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, ~layerMask))
        if (Physics.Raycast(linePointer.transform.position, linePointer.transform.parent.forward, out hit, Mathf.Infinity, layerMask))
        {
                points[1] = hit.point;
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            // store the button in the member var btn
            btn = hit.collider.gameObject.GetComponent<Button>();
            hitBtn = true;
        }
        else
        {
            Physics.Raycast(linePointer.transform.position, linePointer.transform.parent.forward, out hit, Mathf.Infinity);
            points[1] = hit.point;
            lr.startColor = Color.green;
            lr.endColor = Color.green;
        }

        lr.material.color = lr.startColor;
        lr.SetPositions(points);
        return hitBtn;
    }

    // enable/disable left and right line pointers for menu selection
    void SetLinePointersActive(bool state)
    {
        lr_l.enabled = state;
        lr_r.enabled = state;
    }

    void CheckMenuToggle()
    {
        if(OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (UI_panel.activeSelf)
            {
                UI_panel.SetActive(false);
                SetLinePointersActive(false);
            } else
            {
                UI_panel.SetActive(true);
                SetLinePointersActive(true);
            }
        }
    }

    public void OnSaveSetup()
    {
        print("SAVING SETUP");
        dkMan.SaveSetup();
    }

    public void OnLoadSetup()
    {
        print("Loading Setup");
        dkMan.LoadSetup();
    }

    public void OnTrailToggle()
    {
        mp_l.ToggleActive();
        mp_r.ToggleActive();
    }

    // swap button color when toggled
    void ToggleButtonColor(bool state)
    {
        btn.image.color = state ? Color.red : Color.white;
    }


    public void OnCaptureMotion()
    {
        capToggle = !capToggle; // negate bool
        ToggleButtonColor(capToggle);
        captureMotion.ToggleCapture();
    }

    public void OnPlayerMovement()
    {
        playerMovement = !playerMovement;
        ToggleButtonColor(playerMovement);
        playerController.EnableLinearMovement = playerMovement;
    }

    public void OnStickAdjust()
    {
        stickAdjust = !stickAdjust;
        ToggleButtonColor(stickAdjust);
        playerController.EnableLinearMovement = stickAdjust;
    }
}
