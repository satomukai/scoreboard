using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForSFScript : MonoBehaviour {
    public GameObject namePlate;
    public GameObject namePlateForWinner; // 勝ち抜け時用のプレート
    public GameObject scoreBoard; // SFのスコアボードは点数をただ表示するだけ
    public GameObject timerPanel;
    public GameObject rulePanel;
    public GameObject[] reviewPanels; // 問題振り返り用のパネルは3枚
    public TMP_InputField questionIndexFrom; // 何番から
    public TMP_InputField questionIndexTo; //　何問読まれたか

    private List<Player> p; // 参加プレイヤーのリスト
    private GameObject[] namePlates;
    private GameObject[] scoreBoards;
    private TMP_InputField[] points; // 点数を管理

    private bool screenMode = true; // 対戦画面(true)と問題振り返り画面(false)の切り替えに使う
    private int[] winFlag = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int winNum = 0;
    private int from; // questionIndexFromの値
    private int qn; // 読まれた問題数
    private int j;
    private float timeLeft = 2f;
    private bool entryFlag = true; // パネルを順番に表示している最初の間だけtrue
    private static int[] entryOrder = new int[9] { 3, 5, 2, 6, 1, 7, 0, 8, 4 }; // パネルを表示する順番
    private int k = 0; // entryOrder内のインデックス
    private bool winEffectFlag = false; // 勝ち抜け時の表示をする

    void Start() {
        int count = MainControllerScript.players.Count(obj => obj.entry["SF"] == "Entry"); // ルールの参加人数をチェック
        if (count == 9) {
            p = MainControllerScript.players.FindAll(obj => obj.entry["SF"] == "Entry");
            p.Sort((a, b) => a.pictureIndex - b.pictureIndex);
        } else
            p = MainControllerScript.players.GetRange(0, 9);

        // ネームプレートとスコアボードを生成
        namePlates = new GameObject[9];
        scoreBoards = new GameObject[9];
        points = new TMP_InputField[9];
        for (int i = 0; i < 9; i++) {
            // 各種オブジェクトを取得
            namePlates[i] = Instantiate(namePlate, transform.position, Quaternion.Euler(180f, 0f, 0f));
            scoreBoards[i] = Instantiate(scoreBoard, transform.position, transform.rotation);
            points[i] = scoreBoards[i].transform.Find("Canvas/Points").gameObject.GetComponent<TMP_InputField>();

            // プレートの設定
            namePlates[i].transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            namePlates[i].transform.Find("NamePlate/Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            namePlates[i].transform.Find("NamePlate/Canvas/School").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].school + " " + p[i].grade; // 所属と学年を入力
            namePlates[i].transform.Find("NamePlate/RankPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更
            if (p[i].pictureIndex != -1) {
                namePlates[i].transform.Find("Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player" + p[i].pictureIndex + "_SF"); // 写真の設定
            } else {
                namePlates[i].transform.Find("Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("TestImage_SF"); // テスト用
            }
            // スコアボードのボタンの設定
            scoreBoards[i].transform.Find("Canvas/WinButton").gameObject.GetComponent<WinButtonForSFScript>().index = i; // ボタン番号を登録
            scoreBoards[i].transform.Find("Canvas/LoseButton").gameObject.GetComponent<WinButtonForSFScript>().index = i;
            scoreBoards[i].transform.Find("Canvas/WinButton").gameObject.GetComponent<WinButtonForSFScript>().win = true; // 勝ち抜けのボタンか失格のボタンか
            scoreBoards[i].transform.Find("Canvas/LoseButton").gameObject.GetComponent<WinButtonForSFScript>().win = false;
            scoreBoards[i].transform.Find("Canvas/WinButton").gameObject.GetComponent<WinButtonForSFScript>().controllerForSFScript = this; // ボタン側からこちらを参照できるようにする
            scoreBoards[i].transform.Find("Canvas/LoseButton").gameObject.GetComponent<WinButtonForSFScript>().controllerForSFScript = this;

            updateDisplay(i); // 背景色を更新
        }

        // プレートとスコアボードの表示調整
        for (int i = 0; i < 9; i++) {
            namePlates[i].transform.position += new Vector3(-6.1f + (float)(1.525 * i), -0.2f, 0f);
            scoreBoards[i].transform.position += new Vector3(-6.1f + (float)(1.525 * i), -3.2f, 0f);
        }
    }

    public void saveResult() {
        for (int i = 0; i < 9; i++) {
            if (winFlag[i] > 0) {
                p[i].result["SF"] = winFlag[i];
                p[i].entry["F"] = "Entry";
            } else
                p[i].result["SF"] = 0;
        }
    }

    // パネルの状態を切り替える
    public void WinButtonIsClicked(int index, bool win) {
        if (screenMode) {
            if (win)
                winFlag[index] += winNum + 1; // winFlagは勝ち抜け順の情報も保持する
            else
                winFlag[index]--;
            winNum = winFlag.Count(elem => elem > 0);
            updateDisplay(index);
        }
    }

    // 得点パネルを回転
    public void TurnIsClicked() {
        if (screenMode) {
            for (int i = 0; i < 9; i++) {
                if (winFlag[i] == 0) { // プレイ中の人だけを表示
                    scoreBoards[i].transform.Find("ScoreBoard/Canvas/Main").gameObject.GetComponent<TextMeshProUGUI>().text = points[i].text;
                    scoreBoards[i].transform.Find("ScoreBoard").gameObject.GetComponent<Animator>().SetTrigger("Trigger");
                }
            }
        }
    }

    // 勝ち抜けor失格者のパネルを消す
    public void ClearIsClicked() {
        if (screenMode) {
            for (int i = 0; i < 9; i++) {
                if (winFlag[i] < 0)
                    namePlates[i].GetComponent<Animator>().SetTrigger("End");
                if (winFlag[i] != 0)
                    scoreBoards[i].transform.Find("ScoreBoard").gameObject.GetComponent<Animator>().SetTrigger("End");
            }
        }
    }

    // 対戦画面と振り返り画面を切り替える
    public void ChangeIsClicked() {
        if (screenMode) {
            from = int.Parse(questionIndexFrom.text) - 1;
            int to = int.Parse(questionIndexTo.text);
            if (from < to)
                qn = to - from;
            else
                qn = MainControllerScript.allQuestionNum + to - from;
            j = 0;
            timeLeft = 5f;
            for (int i = 0; i < 9; i++)
                namePlates[i].GetComponent<Animator>().SetTrigger("Trigger");
            for (int i = 0; i < 3; i++) {
                if (!reviewPanels[i].activeSelf) reviewPanels[i].SetActive(true);
                else reviewPanels[i].GetComponent<Animator>().SetTrigger("Start");
                reviewPanels[i].transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                reviewPanels[i].transform.Find("Canvas/Question1").gameObject.GetComponent<TextMeshProUGUI>().text = "";
                reviewPanels[i].transform.Find("Canvas/Answer1").gameObject.GetComponent<TextMeshProUGUI>().text = "";
                reviewPanels[i].transform.Find("Canvas/Question2").gameObject.GetComponent<TextMeshProUGUI>().text = "";
                reviewPanels[i].transform.Find("Canvas/Answer2").gameObject.GetComponent<TextMeshProUGUI>().text = "";
            }
            timerPanel.GetComponent<Animator>().SetTrigger("Trigger");
            if (!rulePanel.activeSelf) rulePanel.SetActive(true);
            else rulePanel.GetComponent<Animator>().SetTrigger("Trigger");
            screenMode = false;
        } else {
            MainControllerScript.questionIndex = (from + qn) % MainControllerScript.allQuestionNum; // 問題番号の更新
            questionIndexFrom.text = "";
            questionIndexTo.text = "";
            for (int i = 0; i < 9; i++)
                namePlates[i].GetComponent<Animator>().SetTrigger("Trigger");
            for (int i = 0; i < 3; i++)
                reviewPanels[i].GetComponent<Animator>().SetTrigger("End");
            timerPanel.GetComponent<Animator>().SetTrigger("Trigger");
            timerPanel.transform.Find("Canvas1/TimerText").gameObject.GetComponent<TimerScript>().Reset();
            rulePanel.GetComponent<Animator>().SetTrigger("Trigger");
            screenMode = true;
        }
    }

    public void ZoomIsClicked() {
        int index;
        if (screenMode && (index = Array.IndexOf(winFlag, winNum)) != -1) {
            namePlateForWinner.transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[index].name; // 名前を入力
            if (p[index].pictureIndex != -1) {
                namePlateForWinner.transform.Find("NamePlate/Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player" + p[index].pictureIndex + "_F"); // 写真の設定
            } else {
                namePlateForWinner.transform.Find("NamePlate/Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("TestImage_F"); // テスト用
            }
            namePlateForWinner.GetComponent<Animator>().SetTrigger("Win");
            for (int i = 0; i < 9; i++) {
                namePlates[i].GetComponent<Animator>().SetTrigger("Trigger");
                scoreBoards[i].transform.Find("ScoreBoard").gameObject.GetComponent<Animator>().SetTrigger("Trigger2");
            }
            timerPanel.GetComponent<Animator>().SetTrigger("Trigger");
            timeLeft = 12f;
            winEffectFlag = true;
        }
    }

    public void updateDisplay(int index) {
        Renderer backPanelRenderer = namePlates[index].transform.Find("BackPanel").gameObject.GetComponent<Renderer>();
        TextMeshProUGUI name = namePlates[index].transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI school = namePlates[index].transform.Find("NamePlate/Canvas/School").gameObject.GetComponent<TextMeshProUGUI>();

        if (winFlag[index] > 0) { // 勝ち抜け
            name.color = MainControllerScript.orange;
            school.color = MainControllerScript.orange;
            backPanelRenderer.material.color = MainControllerScript.orange;
        } else
        if (winFlag[index] < 0) { // 負け
            name.color = MainControllerScript.gray;
            school.color = MainControllerScript.gray;
            backPanelRenderer.material.color = MainControllerScript.gray;
        } else {
            name.color = MainControllerScript.white;
            school.color = MainControllerScript.white;
            backPanelRenderer.material.color = MainControllerScript.skyBlue;
        }
    }

    void Update() {
        if (!screenMode) { // 振り返り画面のときだけ
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0) { // 大体10秒で問題を切り替える
                timeLeft = 10f;
                if (j < (qn - 1) / 3 + 1) { // 1度に3問表示
                    for (int i = 0; i < 3; i++) { //　裏面に書き込んでターンオーバー
                        reviewPanels[i].transform.Find("Canvas/Question" + ((j + 1) % 2 + 1).ToString()).gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[(from + 3 * j + i) % MainControllerScript.allQuestionNum].question;
                        reviewPanels[i].transform.Find("Canvas/Answer" + ((j + 1) % 2 + 1).ToString()).gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[(from + 3 * j + i) % MainControllerScript.allQuestionNum].answer;
                        if (3 * j + i + 1 > qn) {
                            reviewPanels[i].transform.Find("Canvas/Question" + ((j + 1) % 2 + 1).ToString()).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                            reviewPanels[i].transform.Find("Canvas/Answer" + ((j + 1) % 2 + 1).ToString()).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                        }
                        reviewPanels[i].GetComponent<Animator>().SetTrigger("Trigger");
                    }
                    j++;
                }
            }
        }
        if (entryFlag) { // パネル表示中の処理
            timeLeft -= Time.deltaTime;
            if (k < 9) {
                if (timeLeft <= 0) {
                    timeLeft = 1f;
                    namePlates[entryOrder[k++]].GetComponent<Animator>().SetTrigger("Start");
                }
            } else {
                timeLeft = 5f;
                timerPanel.SetActive(true);
                entryFlag = false;
            }
        }
        if (winEffectFlag) { //　勝ち抜け者決定時の演出
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0) { // 一定時間経過後に状態復帰
                for (int i = 0; i < 9; i++) {
                    namePlates[i].GetComponent<Animator>().SetTrigger("Trigger");
                    scoreBoards[i].transform.Find("ScoreBoard").gameObject.GetComponent<Animator>().SetTrigger("Trigger2");
                }
                timerPanel.GetComponent<Animator>().SetTrigger("Trigger");
                timeLeft = 5f;
                winEffectFlag = false;
            }
        }
    }

    private string ToOrdinalNumber(int i) {
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




