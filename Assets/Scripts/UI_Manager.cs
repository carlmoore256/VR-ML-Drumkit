using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Material selectionMaterial;

    public GameObject UI_panel;

    public GameObject linePointer_L;
    public GameObject linePointer_R;

    public LayerMask layerMask;

    public Button btn;

    private Image img;

    private LineRenderer lr_l;
    private LineRenderer lr_r;

    //private Vector3[] linePoints_l;
    //private Vector3[] linePoints_r;

    private Vector3 linePointerDist = new Vector3(0, 0, 20);

    CaptureMotion captureMotion;
    // Start is called before the first frame update
    void Start()
    {
        //captureMotion = GetComponent<CaptureMotion>();

        img = GetComponent<Image>();

        UI_panel.SetActive(false);

        // initialize line pointers for when menu pops up
        lr_l = linePointer_L.GetComponent<LineRenderer>();
        lr_r = linePointer_R.GetComponent<LineRenderer>();

        //linePoints_l = new Vector3[2];
        //linePoints_r = new Vector3[2];

        //linePoints_l[0] = linePointer_L.transform.position;
        //linePoints_r[0] = linePointer_R.transform.position;

        //linePoints_l[1] = linePointer_L.transform.position + linePointerDist;
        //linePoints_r[1] = linePointer_R.transform.position + linePointerDist;

        //lr_l.SetPositions(linePoints_l);
        //lr_r.SetPositions(linePoints_r);

        SetLinePointersActive(false);
    }

    void Update()
    {
        CheckMenuToggle();

        if (UI_panel.activeSelf)
        {
            if(RayCastMenu(linePointer_L, lr_l) && OVRInput.Get(OVRInput.Button.Three))
                btn.onClick.Invoke();

            if (RayCastMenu(linePointer_R, lr_r) && OVRInput.Get(OVRInput.Button.One))
                btn.onClick.Invoke();

        }
    }

    bool RayCastMenu(GameObject linePointer, LineRenderer lr)
    {
        bool hitBtn = false;

        Vector3[] points = new Vector3[2];
        points[0] = linePointer.transform.position;

        Ray ray;
        ray = new Ray(linePointer.transform.position, linePointer.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, layerMask))
        {
            points[1] = linePointer.transform.forward + new Vector3(0, 0, hit.distance);
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            // store the button in the member var btn
            btn = hit.collider.gameObject.GetComponent<Button>();
            hitBtn = true;
        }
        else
        {
            points[1] = linePointer.transform.forward + linePointerDist;
            lr.startColor = Color.green;
            lr.endColor = Color.green;
        }

        lr.material.color = lr.startColor;
        lr.SetPositions(points);
        return hitBtn;
    }

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
    }
}
