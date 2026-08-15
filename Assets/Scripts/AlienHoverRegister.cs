using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AlienHoverRegister : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    private AlienHoverInfo alienHoverInfo;

    private void Awake()
    {
        alienHoverInfo = GameObject.Find("AlienInfoBox").GetComponent<AlienHoverInfo>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hovering over: " + eventData.pointerEnter.name);
        alienHoverInfo.currentAlienRole = eventData.pointerEnter.name;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Exiting hover over: " + eventData.pointerEnter.name);
        alienHoverInfo.HideHoverInfo();
    }

    public void OnSelect(BaseEventData eventData)
    {
        alienHoverInfo.selectedAlienRole = eventData.selectedObject.name;
        if (alienHoverInfo.showingSelected)
        {
            alienHoverInfo.ShowHoverInfo(alienHoverInfo.selectedAlienRole);
        }
        Debug.Log("Selecting alien: " + eventData.selectedObject.name);
    }
}
