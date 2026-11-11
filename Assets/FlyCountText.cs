using System;
using TMPro;
using UnityEngine;

public class FlyCountText : MonoBehaviour
{
    public void UpdateText()
    {
        var label = GetComponent<TextMeshProUGUI>();
        if (label == null)
            return;
        label.text = FliesManager.eaten + "/" + FliesManager.count;
    }
}
