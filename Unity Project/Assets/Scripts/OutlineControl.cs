using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineControl : MonoBehaviour
{
    Outline[] outlines;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [SerializeField, Range(0f, 1f)]
    private float outlineTransparency = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        outlines = GetComponentsInChildren<Outline>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var outline in outlines)
        {
            outline.OutlineWidth = outlineWidth;
            Color color = outline.OutlineColor;
            color.a = outlineTransparency;
            outline.OutlineColor = color;
        }
    }

    public void SetOutlineWidth(float width)
    {
        outlineWidth = width;
    }

    public void EnableDisableMeshRender(bool flag)
    {
        foreach (var outline in outlines)
        {
            outline.GetComponent<MeshRenderer>().enabled = flag;
        }
    }
}
