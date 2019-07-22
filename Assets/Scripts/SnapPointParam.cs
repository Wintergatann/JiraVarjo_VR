using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPointParam : MonoBehaviour
{
    private bool snapped;
    private bool overlapping;

    // Start is called before the first frame update
    void Awake()
    {
        snapped = false;
        overlapping = false;
    }

    //Set a bool to know if any snap points are overlapping
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "snappoint") overlapping = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "snappoint") overlapping = false;
    }

    public bool getSnapped()
    {
        return snapped;
    }
    public bool setSnapped(bool set)
    {
        return snapped = set;
    }

    public bool getOverlapping()
    {
        return overlapping;
    }
}
