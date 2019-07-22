using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PostButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public UnityEvent postEvent;

    public float compressX = 0.03f;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        Debug.Log("invoking post...");
        postEvent.Invoke();
        transform.localScale -= new Vector3(compressX, 0, 0);

    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        transform.localScale += new Vector3(compressX, 0, 0);
    }
}
