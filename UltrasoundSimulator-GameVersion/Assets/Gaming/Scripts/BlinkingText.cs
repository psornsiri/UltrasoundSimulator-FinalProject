using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] Text thisText;
    string textToBlink;

    private void Awake()
    {
        textToBlink = thisText.text;
    }

    private void OnEnable()
    {
        StartCoroutine(Blinking());
    }
    IEnumerator Blinking()
    {

        while (true)
        {
            thisText.text = textToBlink;
            yield return new WaitForSeconds(.45f);
            thisText.text = string.Empty;
            yield return new WaitForSeconds(.45f);
        }
    }
}