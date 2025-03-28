using System;
using TMPro;
using UnityEngine;

public class FlyCountText : MonoBehaviour
{
    public void UpdateText()
    {
        GetComponent<TextMeshProUGUI>().text = string.Concat(FliesManager.eaten, "/", FliesManager.count.ToString());
    }
}
