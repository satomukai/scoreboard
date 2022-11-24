using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinButtonForSFScript : MonoBehaviour
{
    public ControllerForSFScript controllerForSFScript;
    public int index; // ボタンのインデックス
    public bool win; // 勝ち抜けのボタンか失格のボタンか

    public void OnClicked() {
        controllerForSFScript.WinButtonIsClicked(this.index, this.win);
    }
}


