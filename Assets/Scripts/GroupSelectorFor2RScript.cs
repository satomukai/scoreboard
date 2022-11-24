using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupSelectorFor2RScript : MonoBehaviour
{
    public GameObject ControllerFor2R;
    public static int group;
    
    public void OnValueChanged(int value)
    {
        // モードが選択されたらシーン送りボタンを表示&パネル生成
        ControllerFor2R.GetComponent<ControllerFor2RScript>().enabled = true;
        group = value;
    }
}
