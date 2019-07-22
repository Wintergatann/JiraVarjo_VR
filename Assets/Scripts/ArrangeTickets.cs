using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrangeTickets : MonoBehaviour
{
    public int columns = 12;
    public int rows = 20;
    public float spaceDiv = 5.5f;
    public float backZ = 0.2f;
    public GameObject backPanel;
    public GameObject parentRef;

    public float yOffset = 5.4f;
    public float zOffset = 0.01f;
    public float xPadding = 0.13f;
    public float yPadding = 0.05f;
    private float panelLength = 0.85f;

    private void ArrangeChildren(Transform[] children)
    {
        int resetFactor = 0;
        for (int i = 0; i < children.Length - resetFactor; i++)
        {
            int row = i / columns;
            int column = i % columns;

            Debug.Log("row" + row);
            Debug.Log("col: " + column);

            if (row > rows)
            {
                Debug.Log("time to make another board");
                GameObject backPanelRef = GameObject.Instantiate(backPanel, new Vector3(parentRef.transform.position.x, parentRef.transform.position.y, parentRef.transform.position.z - backZ), parentRef.transform.rotation);
                resetFactor = i;
                i = 0;

            } else
            {
                children[i].position = new Vector3((-column / spaceDiv) + (parentRef.transform.position.x - xPadding), ((-row / spaceDiv) + yOffset) - yPadding, parentRef.transform.position.z + zOffset);
            }
        }
    }

    private void OnTransformChildrenChanged()
    {
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        ArrangeChildren(children);
    }
}
