using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForCourseChoiceScript : MonoBehaviour
{
    public GameObject namePlate;
    public GameObject courseDisplayPanel;
    private GameObject[] namePlates;
    private Animator[] animators;

    private static Color32[] ruleColors = new Color32[4] { new Color32(0, 128, 0, 255), new Color32(255, 70, 0, 255), new Color32(65, 105, 225, 255), new Color32(186, 85, 211, 255) };  // 各ルールの色
    private int selectedRule = -1;
    private int selectedRuleNum = 0; // 何個のルールが埋まったか
    private int[] ruleCounts = new int[4] { 0, 0, 0, 0 };
    private int[] entryRules = new int[20] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    private List<int[]> history = new List<int[]>();
    private List<Player> p = new List<Player>();
    private bool displayFlag = false; // ルールを画面に出すか

    void Start()
    {
        namePlates = new GameObject[20];
        animators = new Animator[20];

        for (int i = 1; i <= 5; i++)
            p.AddRange(MainControllerScript.players.FindAll(obj => obj.result["2R"] == i)); // 2Rの勝ち抜け順->ペーパー順位で並び替えた20人分のリストを取得
        if (p.Count != 20)
            p = MainControllerScript.players.GetRange(0, 20); // デバッグ用

        // プレートを生成
        for (int i = 0; i < 20; i++)
        {
            namePlates[i] = Instantiate(namePlate, transform.position, transform.rotation);
            animators[i] = namePlates[i].transform.Find("CoursePanel").gameObject.GetComponent<Animator>();

            // 表示位置の調整
            namePlates[i].transform.position = new Vector3((float)(-5.85 + (i % 10) * 1.3), (float)(1.1 - 3.7 * (i / 10)), 0f);
            namePlates[i].transform.Find("Canvas/SetButton").gameObject.transform.position = new Vector3((float)(-5.85 + (i % 10) * 1.3), (float)(-4.3 - 0.6 * (i / 10)), 0f);

            // プレイヤー毎の設定
            namePlates[i].transform.Find("Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            namePlates[i].transform.Find("Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            namePlates[i].transform.Find("Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().color = MainControllerScript.plateColors[p[i].color]; // 順位の色変更
            namePlates[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更  

            // ボタンの設定
            namePlates[i].transform.Find("Canvas/SetButton").gameObject.GetComponent<SetButtonScript>().index = i; // ボタン番号を登録
            namePlates[i].transform.Find("Canvas/SetButton").gameObject.GetComponent<SetButtonScript>().controllerForCourseChoiceScript = this; // ボタン側からこちらを参照できるようにする 
        }

        history.Add(Copy(entryRules));
    }

    public void OnValueChanged(int value)
    {
        if (0 < value && value <= 4)
            selectedRule = value - 1;
    }

    public void OnClicked(int index)
    {
        if (entryRules[index] == -1 && selectedRule != -1 && ruleCounts[selectedRule] < 5)
        { // 対象プレイヤーがルール未決定かつルールが選択されていたら
            entryRules[index] = selectedRule; //参加ルールを変更
            namePlates[index].transform.Find("CoursePanel/Canvas/CourseText").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.rules_3R[selectedRule].Item1; // テキストを変更
            namePlates[index].transform.Find("CoursePanel/Body").gameObject.GetComponent<Renderer>().material.color = ruleColors[selectedRule]; // 色を変更
            ruleCounts[selectedRule]++; // エントリーした人数を増やす
            if (ruleCounts[selectedRule] == 5) // ruleOrderの更新
                MainControllerScript.rules_3R[selectedRule].Item2 = ++selectedRuleNum;
            namePlates[index].transform.Find("CoursePanel").gameObject.SetActive(true); // ルールを表示するパネルをアクティブに
            animators[index].SetTrigger("Trigger"); // アニメーション起動
            history.Add(Copy(entryRules)); // 変更履歴を保存
        }
    }

    public void BackIsClicked()
    {
        if (history.Count > 1)
        { // 履歴が2つ以上あれば
            for (int i = 0; i < 4; i++) ruleCounts[i] = 0;
            // historyの末尾には今の状態が記録されているのでそれより1つ前(index:history.Count-2)をコピー
            for (int i = 0; i < entryRules.Length; i++)
            {
                entryRules[i] = history[history.Count - 2][i];
                if (0 <= entryRules[i] && entryRules[i] < 4)
                    ruleCounts[entryRules[i]]++; // ルールの参加人数を復元
                if (entryRules[i] == -1)
                {
                    namePlates[i].transform.Find("CoursePanel").gameObject.transform.localScale = new Vector3(0f, 0f, 1f);
                    namePlates[i].transform.Find("CoursePanel").gameObject.SetActive(false); // ルールを表示するパネルを非アクティブに
                }
            }
            history.RemoveAt(history.Count - 1); // historyの末尾を削除
        }
    }

    public void EndIsClicked()
    {
        if (ruleCounts.Sum() == 20)
        {
            for (int i = 0; i < 20; i++)
                p[i].entry["3R"] = MainControllerScript.rules_3R[entryRules[i]].Item1;
        }
    }

    public void ZoomIsClicked()
    {
        if (selectedRule != -1)
        {
            displayFlag = !displayFlag;
            if (displayFlag)
                courseDisplayPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.rules_3R[selectedRule].Item1;
            courseDisplayPanel.GetComponent<Animator>().SetTrigger("Trigger");
        }
    }

    private int[] Copy(int[] source)
    {
        int[] tmp = new int[source.Length];
        for (int i = 0; i < source.Length; i++)
            tmp[i] = source[i];
        return tmp;
    }

    private string ToOrdinalNumber(int i)
    {
        if (11 <= i && i <= 13)
            return i + "th";
        if (i % 10 == 1)
            return i + "st";
        else if (i % 10 == 2)
            return i + "nd";
        else if (i % 10 == 3)
            return i + "rd";
        else
            return i + "th";
    }
}
