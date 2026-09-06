using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class AlienHoverRegister : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    private AlienHoverInfo alienHoverInfo;

    private void Awake()
    {
        alienHoverInfo = GameObject.Find("AlienInfoBox").GetComponent<AlienHoverInfo>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        alienHoverInfo.currentAlienRole = eventData.pointerEnter.name;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        alienHoverInfo.HideHoverInfo();
    }

    public void OnSelect(BaseEventData eventData)
    {
        alienHoverInfo.selectedAlienRole = eventData.selectedObject.name;
        if (alienHoverInfo.showingSelected)
        {
            alienHoverInfo.ShowHoverInfo(alienHoverInfo.selectedAlienRole);
        }
    }
}
