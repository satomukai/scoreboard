using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockScript : MonoBehaviour
{

    private TextMeshProUGUI clockText;
    bool startFlag;
    void Start()
    {
        clockText = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        clockText.text = DateTime.Now.ToString("HH:mm:ss");
    }
}
