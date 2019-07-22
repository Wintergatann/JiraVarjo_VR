using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;


public class Menu : MonoBehaviour
{

    public GameObject buttonPrefab;
    public GameObject panelRef;
    public GameObject networkRef;
    public GameObject textRef;
    private string projectURL = "https://jiraapi2.herokuapp.com/projects";
    public string defaultProjID = "10000";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetProjects(projectURL));
        networkRef.GetComponent<NetworkMetadata>().SetProjID(defaultProjID);
    }

    public IEnumerator GetProjects(string uri)
    {
        Debug.Log("Getting projects...");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            //Request and wait for page
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nRecieved: " + webRequest.downloadHandler.text);
                LoadProjects(webRequest.downloadHandler.text);
            }
        }
    }

    public void LoadProjects(string json)
    {
        textRef.GetComponent<Text>().text = "Projects: (select one and create or join a room)";
        var pjo = JObject.Parse(json);
        Debug.Log("loaded");
        JArray pjoBoardArray = (JArray)pjo["projects"];
        for (int i = 0; i < pjoBoardArray.Count; i++)
        {
            Debug.Log("YEET: " + pjoBoardArray[i]["id"]);
            GameObject button = GameObject.Instantiate(buttonPrefab);
            button.transform.SetParent(panelRef.transform);
            string projID = pjoBoardArray[i]["id"].Value<string>();
            button.GetComponentInChildren<Text>().text = projID;

            button.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(projID));
        }
    }

    public void OnButtonClick(string projectID)
    {
        networkRef.GetComponent<NetworkMetadata>().SetProjID(projectID);
    }
}
