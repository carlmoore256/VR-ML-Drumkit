using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableProp : MonoBehaviour
{
    // apply to an object that can be grabbed and moved around

    Renderer[] allRenderers;
    Material[] allMaterials;
    Material selectMat;
    Coroutine updateRenderer;
    public Vector3 currentScale;

    private Vector3 mGrabOffset;

    public bool m_Repositioning = false;

    void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>();

        allMaterials = new Material[allRenderers.Length];

        for(int i = 0; i < allMaterials.Length; i++)
            allMaterials[i] = allRenderers[i].material;
            
        selectMat = GameObject.Find("UI").GetComponent<UI_Manager>().selectionMaterial;
        currentScale = transform.localScale;
    }

    // allows user to grab and move prop; grab flag captures initial offset
    public void MoveProp(Vector3 newPosition, bool grabFlag)
    {
        m_Repositioning = true;

        if (grabFlag)
            mGrabOffset = transform.position-newPosition;

        if (updateRenderer != null)
            StopCoroutine(updateRenderer);
        updateRenderer = StartCoroutine(HighlightProp());

        transform.position = mGrabOffset + newPosition;
    }


    public void ScaleProp(float scale)
    {
        // Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(currentScale.x + scale, currentScale.y + scale, currentScale.z + scale);
        currentScale = transform.localScale;
    }

    public void SelectionHover()
    {
        if(updateRenderer != null)
            StopCoroutine(updateRenderer);
        updateRenderer = StartCoroutine(HighlightProp());
    }

    IEnumerator HighlightProp()
    {
        foreach(Renderer rend in allRenderers)
            rend.material = selectMat;

        yield return new WaitForSeconds(0.1f);

        for(int i = 0; i < allRenderers.Length; i++)
            allRenderers[i].material = allMaterials[i];

        yield return new WaitForSeconds(0.1f);
        m_Repositioning = false;

    }
}
