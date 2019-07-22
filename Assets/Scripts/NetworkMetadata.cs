using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMetadata : MonoBehaviour
{
    private string project_ID;

    public void SetProjID(string _projID)
    {
        project_ID = _projID;
    }

    public string GetProjID()
    {
        return project_ID;
    }
}
