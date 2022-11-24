using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ControllerForSF_EntryScript : MonoBehaviour
{
    public GameObject namePlate;
    private List<Player> p = new List<Player>(); // 参加プレイヤーのリスト
    private GameObject[] namePlates;
    private GameObject[] scoreBoards;
    private string[] titles = new string[8];
    private bool updateFlag = false;
    private int sceneCounter = 0;
    private static int[] entryOrder = new int[8] { 7, 3, 4, 2, 5, 1, 6, 0 }; // パネルを表示する順番

    void Start()
    {
        using (StreamReader sr = new StreamReader("./Assets/Data/titles.csv"))
        {
            string line = sr.ReadLine(); // 1行とばす
            line = sr.ReadLine();
            titles = line.Split(',');
        }

        for (int i = 0; i < MainControllerScript.players.Count; i++)
            Debug.Log(MainControllerScript.players[i].pictureIndex);

        int count = MainControllerScript.players.Count(obj => obj.entry["SF"] == "Pass"); // ルールの参加人数をチェック
        if (count == 9)
        {
            p = MainControllerScript.players.FindAll(obj => obj.entry["SF"] == "Pass");
            p.Sort((a, b) => a.pictureIndex - b.pictureIndex);
            p.RemoveAt(4); // pictureIndex=5のプレイヤー(=敗者復活通過者)を削除
        }
        else
            p = MainControllerScript.players.GetRange(0, 8);

        // ネームプレートとスコアボードを生成
        namePlates = new GameObject[8];
        for (int i = 0; i < 8; i++)
        {
            // 各種オブジェクトを取得
            namePlates[i] = Instantiate(namePlate, transform.position + new Vector3(0f, -0.5f, 0f), transform.rotation);

            // プレートの設定
            namePlates[i].transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            namePlates[i].transform.Find("NamePlate/Canvas/Title").gameObject.GetComponent<TextMeshProUGUI>().text = titles[i]; // 名前を入力
            namePlates[i].transform.Find("NamePlate/Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("player" + p[i].pictureIndex + "_F"); // 写真の設定
        }

    }

    public void NextIsClicked()
    {
        if (!updateFlag)
        {
            sceneCounter++;
            updateFlag = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (updateFlag)
        {
            if (1 <= sceneCounter && sceneCounter <= 8)
                namePlates[entryOrder[sceneCounter - 1]].GetComponent<Animator>().SetTrigger("Trigger");
            updateFlag = false;
        }
    }
}
