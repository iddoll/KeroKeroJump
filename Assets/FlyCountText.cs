using System;
using TMPro;
using UnityEngine;

public class FlyCountText : MonoBehaviour
{
    public void UpdateText()
    {
        GetComponent<TextMeshProUGUI>().text = string.Concat("0", "/", FliesManager.count.ToString());
    }
}
