using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMetadata : MonoBehaviour
{
    //These are set in ParseJSON
    public string swimlaneid;
    public string description;
    public string reporter;
    public string creator;
    public string key;
    public string issueType;
    public string id;
    public string summary;
    public string priority;

    //This is set when a panel enters or exits a collider on a ticket board
    public string swimlane = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "swimlane")
        {
            swimlane = other.name;
            Debug.Log("lane: " + swimlane);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "swimlane") swimlane = "";
    }

}
