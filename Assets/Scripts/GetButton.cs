using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GetButton : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public UnityEvent getEvent;
    public float compressX = 0.03f;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        getEvent.Invoke();
        transform.localScale -= new Vector3(compressX, 0, 0);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        transform.localScale += new Vector3(compressX, 0, 0);
    }
}
