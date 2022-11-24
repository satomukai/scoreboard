using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupSelectorForExR2Script : MonoBehaviour {
    public GameObject ControllerForExR2;
    public static int group;

    public void OnValueChanged(int value) {
        // モードが選択されたらシーン送りボタンを表示&パネル生成
        ControllerForExR2.GetComponent<ControllerForExR2Script>().enabled = true;
        group = value;
    }
}
