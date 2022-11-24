using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseSelectorFor3RScript : MonoBehaviour
{
    public GameObject ControllerFor3R;
    public static (string, int) rule;

    public void OnValueChanged(int value)
    {
        // モードが選択されたらシーン送りボタンを表示&パネル生成
        ControllerFor3R.GetComponent<ControllerFor3RScript>().enabled = true;
        rule = MainControllerScript.rules_3R[value - 1];
    }
}

