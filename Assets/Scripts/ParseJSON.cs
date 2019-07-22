using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public class ParseJSON : MonoBehaviour
{

    public GameObject panelPrefab;
    public GameObject boardPrefab;
    public GameObject boardObjectsPrefab;
    public GameObject _controllerR;
    public GameObject _controllerL;

    //Fill this when you look through swimlane object and generate the boards
    public GameObject[] boardArr;

    public float padding;
    public float panelLength;

    public float boardZ;
    public float boardY;

    private string url;
    private string ojson;
    private GameObject[] metadataArr;

    public void Start()
    {
        string projID = GameObject.FindGameObjectWithTag("networkdata").GetComponent<NetworkMetadata>().GetProjID();
        url = "https://jiraapi2.herokuapp.com/tickets/" + projID;
        GetEvent();
    }

    public void GetEvent()
    {
        StartCoroutine(Parse(url));
    }

    public IEnumerator Parse(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            //Request and wait for page
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else
            {
                Debug.Log(pages[page] + ":\nRecieved: " + webRequest.downloadHandler.text);
                LoadJson(webRequest.downloadHandler.text);
            }
        }
    }

    public void LoadJson(string json)
    {
        ojson = json;
        var jo = JObject.Parse(json);
        Debug.Log("loadjson worked");

        //Make the boards + swimlanes
        JArray joBoardArray = (JArray)jo["project"][0]["Swimlanes"];

        //If you needed to make a dictionary for looking at key value pairs then you would use this.
        //IDictionary<string, object> dict = joBoardArray.ToDictionary(k => ((JObject)k).Properties().First().Name, v => v.Values().First().Value<object>());

        int boardC = -1;
        
        //foreach (var kv in dict)
        for (int z = 0; z < joBoardArray.Count; z++)
        {
            boardC++;
            boardArr = new GameObject[joBoardArray.Count];
            //Debug.Log("DICT: " + kv.Key + ", " + kv.Value);
            GameObject boardRef = GameObject.Instantiate(boardPrefab);
            GameObject boardObjectsRef = GameObject.Instantiate(boardObjectsPrefab);

            boardArr[z] = boardRef;
            //boardRef.name = kv.Key;
            foreach (Transform child in boardRef.transform)
            {
                if (child.tag == "swimlane") child.name = jo["project"][0]["Swimlanes"][z]["id"].Value<string>();
            }
            //loop through the empty which has the swimlane text
            foreach (Transform _child in boardObjectsRef.transform)
            {
                if (_child.tag == "boardcanvas")
                {
                    foreach (Transform grandchild in _child.transform)
                    {
                        if (grandchild.tag == "boardtext") grandchild.GetComponent<TextMeshProUGUI>().text = jo["project"][0]["Swimlanes"][z]["name"].Value<string>();
                    }
                }
            }

            //Arrange the boards with even spacing and center them around x = 0
            float y = joBoardArray.Count / 2;
            if (y % 2 == 0)
            {
                boardRef.transform.position = new Vector3((y - boardC) * (panelLength + padding), boardY, boardZ);
            }
            else
            {
                boardRef.transform.position = new Vector3(((y - boardC) * panelLength) + (((y - boardC)) * padding) + (0.4f * joBoardArray.Count), boardY, boardZ);
            }

            boardObjectsRef.transform.position = boardRef.transform.position;
        }

        //Make the panels
        JArray joPanelArray = (JArray)jo["project"][0]["Issues"];
        metadataArr = new GameObject[joPanelArray.Count];

        Debug.Log("count: " + joPanelArray.Count);
        for (int i = 0; i < joPanelArray.Count; i++)
        {
            GameObject panelRef = GameObject.Instantiate(panelPrefab);
            PanelMetadata metadata = panelRef.GetComponent<PanelMetadata>();
            TriggerEvent trigger = panelRef.GetComponent<TriggerEvent>();

            //This uses the Json.Net plugin
            metadata.swimlaneid = jo["project"][0]["Issues"][i]["swimlane"][0]["id"].Value<string>();
            metadata.description = jo["project"][0]["Issues"][i]["description"].Value<string>();
            metadata.reporter = jo["project"][0]["Issues"][i]["reporter"].Value<string>();
            metadata.creator = jo["project"][0]["Issues"][i]["creator"].Value<string>();
            metadata.key = jo["project"][0]["Issues"][i]["key"].Value<string>();
            metadata.issueType = jo["project"][0]["Issues"][i]["issueType"].Value<string>();
            metadata.id = jo["project"][0]["Issues"][i]["id"].Value<string>();
            metadata.summary = jo["project"][0]["Issues"][i]["summary"].Value<string>();
            metadata.priority = jo["project"][0]["Issues"][i]["priority"].Value<string>();

            metadataArr[i] = panelRef;

            GameObject matchingBoard = GameObject.Find(metadata.swimlaneid);

            foreach (Transform sibling in matchingBoard.transform.parent)
            {
                if (sibling.tag == "boardobj") panelRef.transform.SetParent(sibling);
            }

            foreach (Transform child in panelRef.transform)
            {
                if (child.tag == "paneltext")
                {
                    Debug.Log("found text");
                    child.GetComponent<TextMeshPro>().text = metadata.summary;
                }
            }
        }
    }

    //This is invoked by the save button
    public void SaveJson()
    {
        Debug.Log("Json being saved");
        var ojo = JObject.Parse(ojson);
        JArray ojoArr = (JArray)ojo["project"][0]["Issues"];
        for (int i = 0; i < ojoArr.Count; i++)
        {
            //Since right now we can only change the swimlanedid I am just taking the original json and setting the new swimlaneid values
            ojo["project"][0]["Issues"][i]["swimlaneid"] = metadataArr[i].GetComponent<PanelMetadata>().swimlane;
        }
        string _json = ojo.ToString();
        Debug.Log("json: " + _json);
        StartCoroutine(Upload(_json, url));
        //StartCoroutine(Upload("https://jiraapi2.herokuapp.com/tickets/" + "10003" + "Nicholas Woodward"));
    }

    //This is working on the Unity end, but the Jira API will not give us permission to post currently.
    public IEnumerator Upload(string json, string _url)
    {
        Debug.Log("Uploading...");
        byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(json);
        //UnityWebRequest www = UnityWebRequest.Put(_url, form);
        UnityWebRequest www = UnityWebRequest.Put(_url, jsonData);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("ERROR: " + www.error);
        }
        else
        {
            Debug.Log("Received: " + www.downloadHandler.text);
            Debug.Log("Form upload complete.");
        }
    }
}