using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExRoundEntryScript : MonoBehaviour
{
    public TMP_InputField inputField;
    public void OnValueChanged()
    {
        string text = inputField.text;
        string[] array = text.Split(','); // ペーパー順位
        int[] ranks = new int[8];
        int pNum = MainControllerScript.players.Count; // 全参加者数

        if (0 < array.Length && array.Length <= 8)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (!int.TryParse(array[i], out ranks[i])) return; // 正しいデータでなければやり直し
                if (ranks[i] > pNum) return;
            }
            for (int i = 0; i < array.Length; i++)
                MainControllerScript.players[ranks[i] - 1].entry["Ex"] = "Pass"; // playersはペーパー順位で並んでいる
            this.gameObject.SetActive(false);
        }
    }
}