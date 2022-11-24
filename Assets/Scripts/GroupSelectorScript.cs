using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupSelectorScript : MonoBehaviour
{
    public GameObject nextButton;
    public GameObject turnOverController;
    public static int group;
    
    public void OnValueChanged(int value)
    {
        // モードが選択されたらシーン送りボタンを表示&パネル生成
        turnOverController.GetComponent<TurnOverControllerScript>().enabled = true;
        nextButton.SetActive(true);
        group = value;
    }
}
