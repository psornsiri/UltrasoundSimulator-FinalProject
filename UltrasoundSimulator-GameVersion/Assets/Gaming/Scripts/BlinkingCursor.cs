using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class BlinkingCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject mouse_cursor;
    private bool mouse_over = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mouse_over != true)
        {
            mouse_cursor.SetActive(true);
            mouse_over = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_cursor.SetActive(false);
        mouse_over = false;
    }
}
