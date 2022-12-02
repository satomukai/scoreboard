using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForFScript : GameControllerScript
{
    public GameObject namePlateForWinner;
    public GameObject champButton;

    private bool timeOutFlag = true;
    private bool endFlag = false; // Endが押されるまでTimeOutを押せないように
    private int selectedRule = -1; // 選択中のルールを0-6で(5o1x, 5o7x, 5o5休, 変則4o3x, 7ptアタサバ, 7o3x, 3Combo shot)
    private int[] selectedRules = new int[7] { 0, 0, 0, 0, 0, 0, 0 }; // 選択中は1, 終了したら2
    private int[] gotSets = new int[3] { 0, 0, 0 }; // 獲得セット数

    void Start()
    {
        Entry("F", "Entry"); // players.entry[key] == value のプレイヤーが参加
        // ネームプレートとスコアボードを生成
        pNum = p.Count;
        namePlates = new GameObject[pNum];
        scoreBoards = new GameObject[pNum];
        for (int i = 0; i < pNum; i++)
        {
            // 各種オブジェクトを取得
            namePlates[i] = Instantiate(namePlate, transform.position, transform.rotation);
            scoreBoards[i] = Instantiate(scoreBoard, transform.position, transform.rotation);

            // プレートの設定
            namePlates[i].transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            namePlates[i].transform.Find("NamePlate/Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            namePlates[i].transform.Find("NamePlate/Canvas/School").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].school + " " + p[i].grade; // 所属と学年を入力
            namePlates[i].transform.Find("NamePlate/RankPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更
            if (p[i].pictureIndex != -1)
                namePlates[i].transform.Find("Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player" + p[i].pictureIndex + "_F"); // 写真の設定
            else
                namePlates[i].transform.Find("Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("TestImage_F"); // テスト用

            // スコアボードのボタンの設定
            scoreBoards[i].transform.Find("Canvas/CorrectButton").gameObject.GetComponent<CorrectButtonScript>().index = i; // ボタン番号を登録
            scoreBoards[i].transform.Find("Canvas/WrongButton").gameObject.GetComponent<CorrectButtonScript>().index = i;
            scoreBoards[i].transform.Find("Canvas/CorrectButton").gameObject.GetComponent<CorrectButtonScript>().correct = true; // 正解のボタンか誤答のボタンか
            scoreBoards[i].transform.Find("Canvas/WrongButton").gameObject.GetComponent<CorrectButtonScript>().correct = false;
            scoreBoards[i].transform.Find("Canvas/CorrectButton").gameObject.GetComponent<CorrectButtonScript>().gameControllerScript = this; // ボタン側からこちらを参照できるようにする
            scoreBoards[i].transform.Find("Canvas/WrongButton").gameObject.GetComponent<CorrectButtonScript>().gameControllerScript = this;
        }

        // 問題パネルに問題を書き込む
        questionPanel.transform.Find("Canvas/Question").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[MainControllerScript.questionIndex].question;
        questionPanel.transform.Find("Canvas/Answer").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[MainControllerScript.questionIndex].answer;
        questionCheck.GetComponent<TextMeshProUGUI>().text = "次の問題: " + MainControllerScript.questions[MainControllerScript.questionIndex].question.Substring(0, 10) + "...";

        // プレートとスコアボードの表示位置を調整
        for (int i = 0; i < pNum; i++)
        {
            namePlates[i].transform.position += new Vector3((float)(4.5 * (i - 1)), 0f, 0f);
            scoreBoards[i].transform.position += new Vector3((float)(4.5 * (i - 1)), -3f, 0f);
        }

    }

    // ルールが選択されたらゲーム管理クラスのインスタンスを生成する
    public void RuleIsSelected(int value)
    {
        if (selectedRules[value - 1] != 0)
            return;
        selectedRule = value - 1;
        selectedRules[selectedRule] = 1;
        switch (selectedRule)
        {
            case 0:
                game = new StandardRule(pNum: pNum, winLimit: 1, correctLimit: 5, wrongLimit: 1);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 5○1×";
                break;
            case 1:
                game = new StandardRule(pNum: pNum, winLimit: 1, correctLimit: 5, wrongLimit: 7);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 5○7×";
                break;
            case 2:
                game = new StandardRule(pNum: pNum, winLimit: 1, correctLimit: 5, wrongLimit: -1, freezeNum: 5);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 5○5休";
                break;
            case 3:
                game = new IrregularRule(pNum: pNum, winLimit: 1, correctLimit: 4, wrongLimit: 3);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 変則4○3×";
                break;
            case 4:
                game = new AttackSurvival(pNum: pNum, winLimit: 1, hp: 7, minusByOthersCorrectAnswer: 1, minusByMyWrongAnswer: 2);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 7Point AS";
                break;
            case 5:
                game = new StandardRule(pNum: pNum, winLimit: 1, correctLimit: 7, wrongLimit: 3);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 7○3×";
                break;
            case 6:
                game = new ComboShot(pNum: pNum, winLimit: 1, correctLimit: 3, wrongLimit: 3);
                questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "Final:  The Trinity - 3Combo Shot";
                break;
        }
        game.InitialUpdate();
        game.makeHistory();
        updateDisplay(); // スコア表示を更新
        for (int i = 0; i < pNum; i++)
        {
            scoreBoards[i].GetComponent<Animator>().SetBool("Flag", false);
            scoreBoards[i].transform.Find("FreezePanel").gameObject.SetActive(false);
        }

    }

    // タイムアウト中に次のルールに変える
    public void TimeOutIsClicked()
    {
        if (!timeOutFlag)
        {
            if (endFlag)
            {
                for (int i = 0; i < pNum; i++)
                {
                    scoreBoards[i].transform.Find("Canvas/Combo").gameObject.SetActive(false);
                    scoreBoards[i].GetComponent<Animator>().SetBool("Flag", false);
                    scoreBoards[i].GetComponent<Animator>().SetTrigger("Trigger");
                }
                timeOutFlag = true;
            }
        }
        else
        {
            if (selectedRule != -1)
            {
                for (int i = 0; i < pNum; i++)
                    scoreBoards[i].GetComponent<Animator>().SetTrigger("Trigger");
                endFlag = false;
                timeOutFlag = false;
            }
        }
    }

    // 対戦結果をゲームに反映させる
    public override void saveResult()
    {
        for (int i = 0; i < pNum; i++)
        {
            if (gotSets[i] == 3)
                p[i].result["F"] = 1;
            else
                p[i].result["F"] = 0;
        }
    }

    public override void EndIsClicked()
    {
        if (updateFlag && !endFlag)
        {
            game.End(); // 勝ち抜け者を決める
            for (int i = 0; i < pNum; i++)
                gotSets[i] += game.scores[i].winFlag; // 獲得セット数を更新
            updateDisplay(); // 表示を更新
            selectedRules[selectedRule] = 2;
            selectedRule = -1;
            if (Array.IndexOf(gotSets, 3) != -1)
            { // 3セット獲得したプレイヤーが出たら
                saveResult();
                champButton.SetActive(true);
            }
            updateFlag = true;
            endFlag = true;
        }
    }

    public override void updateDisplay()
    {
        // 変えないといけないのは正解数, 誤答数の表示&色と, バーと背景パネルの色
        TextMeshProUGUI mainTMP, subTMP, comboTMP;
        Renderer barRenderer;
        for (int i = 0; i < pNum; i++)
        {
            mainTMP = scoreBoards[i].transform.Find("Canvas/Main").gameObject.GetComponent<TextMeshProUGUI>();
            subTMP = scoreBoards[i].transform.Find("Canvas/Sub").gameObject.GetComponent<TextMeshProUGUI>();
            comboTMP = scoreBoards[i].transform.Find("Canvas/Combo").gameObject.GetComponent<TextMeshProUGUI>();
            barRenderer = scoreBoards[i].transform.Find("bar").gameObject.GetComponent<Renderer>();
            if (game.scores[i].winFlag > 0)
            { // 勝ち抜け
                mainTMP.text = Convert.ToString(game.scores[i].point);
                mainTMP.color = MainControllerScript.orange;
                subTMP.color = MainControllerScript.orange;
                barRenderer.material.color = MainControllerScript.orange;
            }
            else if (game.scores[i].loseFlag)
            { // 負け
                mainTMP.text = "FAILED";
                mainTMP.color = MainControllerScript.gray;
                subTMP.color = MainControllerScript.gray;
                barRenderer.material.color = MainControllerScript.gray;
            }
            else
            {
                mainTMP.text = Convert.ToString(game.scores[i].point);
                mainTMP.color = MainControllerScript.white;
                subTMP.color = MainControllerScript.white;
                barRenderer.material.color = MainControllerScript.white;
            }
            switch (selectedRule)
            {
                case 0:
                    subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 1);
                    break;
                case 1:
                    subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 7, foldingFlag: false);
                    break;
                case 2:
                    subTMP.text = "";
                    break;
                case 3:
                    subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 3);
                    break;
                case 4:
                    subTMP.text = "";
                    break;
                case 5:
                    subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 3);
                    break;
                case 6:
                    subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 3);
                    if (game.scores[i].winFlag > 0 || game.scores[i].loseFlag)
                        scoreBoards[i].transform.Find("Canvas/Combo").gameObject.SetActive(false);
                    else
                        scoreBoards[i].transform.Find("Canvas/Combo").gameObject.SetActive(true);
                    comboTMP.text = Convert.ToString(game.scores[i].consecutiveAnswersNum);
                    comboTMP.color = MainControllerScript.white;
                    break;
            }
            namePlates[i].transform.Find("Canvas/Stars").gameObject.GetComponent<TextMeshProUGUI>().text = new string('★', gotSets[i]) + new string('☆', 3 - gotSets[i]);
        }
        if (selectedRule == 2)
        {
            for (int i = 0; i < pNum; i++)
            {
                if (game.scores[i].freezeTime > 0)
                {
                    if (!scoreBoards[i].transform.Find("FreezePanel").gameObject.activeSelf)
                        scoreBoards[i].transform.Find("FreezePanel").gameObject.SetActive(true);
                    scoreBoards[i].transform.Find("FreezePanel/Canvas/FreezeTime").gameObject.GetComponent<TextMeshProUGUI>().text = "Freeze\n" + game.scores[i].freezeTime.ToString();
                }
                else
                {
                    if (scoreBoards[i].transform.Find("FreezePanel").gameObject.activeSelf)
                        scoreBoards[i].transform.Find("FreezePanel").gameObject.SetActive(false);
                }
            }
        }
    }

    // 優勝者の演出
    public void ChampEffect()
    {
        int index = Array.IndexOf(gotSets, 3);
        namePlateForWinner.SetActive(true);
        namePlateForWinner.transform.Find("NamePlate/Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[index].name;
        if (p[index].pictureIndex != -1)
        {
            namePlateForWinner.transform.Find("NamePlate/Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Player" + p[index].pictureIndex + "_F");
        }
        else
        {
            namePlateForWinner.transform.Find("NamePlate/Canvas/Image").gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("TestImage_F");
        }
        for (int i = 0; i < 3; i++)
        {
            namePlates[i].GetComponent<Animator>().SetTrigger("Trigger");
            scoreBoards[i].GetComponent<Animator>().SetTrigger("Trigger");
        }
    }

    void Update()
    {
        // 連答状態によってアニメーションを変える
        if (selectedRule == 6 && animationFlag)
        {
            for (int i = 0; i < pNum; i++)
            {
                if (game.scores[i].consecutiveAnswersNum > 0 && game.scores[i].winFlag == 0 && !game.scores[i].loseFlag)
                    scoreBoards[i].GetComponent<Animator>().SetBool("Flag", true);
                else
                    scoreBoards[i].GetComponent<Animator>().SetBool("Flag", false);
            }
            animationFlag = false;
        }
    }

}




