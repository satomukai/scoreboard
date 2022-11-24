using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectButtonScript : MonoBehaviour
{
    public GameControllerScript gameControllerScript;
    public int index; // ボタンのインデックス
    public bool correct; // 正解のボタンか誤答のボタンか

    public void OnClicked() {
        gameControllerScript.OnClicked(this.index, this.correct);
    }
}
