using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Carousel : MonoBehaviour, IPointerDownHandler//, IPointerClickHandler
{
    //***Given the recent board size changes, I don't believe that this will be needed anymore.


    private GameObject[] ticketBoardArr;
    private GameObject[] ticketSnapArr;

    //This will be the number of boards we instantiate
    private int ticketBoardNum = 3;

    //temp
    public GameObject ticketBoard1;
    public GameObject ticketBoard2;
    public GameObject ticketBoard3;

    public GameObject ticketSnap1;
    public GameObject ticketSnap2;
    public GameObject ticketSnap3;

    // Start is called before the first frame update
    void Start()
    {
        //Change this later to populate the array with the ticket boards we actually instantiate
        // and make sure that the instantiated objects are snapped to snappoints
        ticketBoardArr = new GameObject[ticketBoardNum];

        ticketSnapArr = new GameObject[ticketBoardNum];
    }

    //This script "carousels" the panels so you can view all the tickets even if they don't all fit on one board.
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        GameObject[] tempArr = new GameObject[ticketBoardArr.Length];
        for (int i = 0; i < ticketBoardArr.Length; i++)
        {
            if (i < ticketBoardArr.Length - 1)
            {
                //if the board is anything other than the last element of the array
                ticketBoardArr[i].transform.position = ticketSnapArr[i + 1].transform.position;

                tempArr[i + 1] = ticketBoardArr[i];
            } else
            {
                //if the board is the last element of the array
                ticketBoardArr[i].transform.position = ticketSnapArr[0].transform.position;
                tempArr[0] = ticketBoardArr[i];
            }

            
        }

        for (int x = 0; x < tempArr.Length; x++)
        {
            ticketBoardArr[x] = tempArr[x];
        }

        Debug.Log("arr1: " + ticketBoardArr[0]);
        Debug.Log("arr2: " + ticketBoardArr[1]);
        Debug.Log("arr3: " + ticketBoardArr[2]);

    }
}
