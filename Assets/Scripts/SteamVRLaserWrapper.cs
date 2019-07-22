using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;
//using Varjo;

public class SteamVRLaserWrapper : MonoBehaviour
{
    public SteamVR_LaserPointer laserPointer;

    private void Awake()
    {
        laserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        laserPointer.PointerIn += OnPointerIn;
        laserPointer.PointerOut += OnPointerOut;
        laserPointer.PointerClick += OnPointerClick;
        laserPointer.PointerDown += OnPointerDown;
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
        if (clickHandler == null)
        {
            return;
        }


        clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void OnPointerDown(object sender, PointerEventArgs e)
    {
        IPointerDownHandler downHandler = e.target.GetComponent<IPointerDownHandler>();
        if (downHandler == null)
        {
            return;
        }

        if (e.target.tag == "panel")
        {
            TriggerEvent panelScr = e.target.GetComponent<TriggerEvent>();
            Debug.Log("panelscr: " + panelScr);
            panelScr.ReceiveController(e);
        }

        downHandler.OnPointerDown(new PointerEventData(EventSystem.current));
    }

    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
        if (pointerExitHandler == null)
        {
            return;
        }

        pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler == null)
        {
            return;
        }

        pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}