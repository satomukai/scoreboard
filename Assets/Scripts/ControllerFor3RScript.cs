using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerFor3RScript : GameControllerScript
{
    public (string, int) rule;
    private Animator[] scoreBoardAnimators; // 連答エフェクトに関するScoreBoardsのAnimatorを管理
    void Start()
    {
        rule = CourseSelectorFor3RScript.rule;

        base.Initialize("3R", rule.Item1);

        // プレートとスコアボードの表示調整
        for (int i = 0; i < pNum; i++)
        {
            namePlates[i].transform.position += new Vector3(-5.5f + (float)(2.0 * i), -0.4f, 0f);
            scoreBoards[i].transform.position += new Vector3(-5.5f + (float)(2.0 * i), -3f, 0f);
            if (rule.Item1 != "Swedish10" && rule.Item1 != "10○10×") // 表示に一行しか使わないルールはフォントサイズを大きくする
                scoreBoards[i].transform.Find("Canvas/Sub").gameObject.GetComponent<TextMeshProUGUI>().fontSize = 0.4f;
        }

        // ルール特有のオブジェクトの設定
        scoreBoardAnimators = new Animator[pNum];
        for (int i = 0; i < pNum; i++)
            scoreBoardAnimators[i] = scoreBoards[i].GetComponent<Animator>();

        // ゲーム管理クラスのインスタンスを生成
        if (rule.Item1 == "10up-down")
            game = new UpDown(pNum: pNum, winLimit: 2, correctLimit: 10); // 10up-down
        else if (rule.Item1 == "Freeze10")
            game = new Freeze(pNum: pNum, winLimit: 2, correctLimit: 10); // Freeze10
        else if (rule.Item1 == "Swedish10")
            game = new Swedish10(pNum: pNum, winLimit: 2); // Swedish10
        else if (rule.Item1 == "10by10")
            game = new By(pNum: pNum, winLimit: 2, n: 10, wrongLimit: 6); // 10by10(6×で失格)
        else if (rule.Item1 == "10○10×")
            game = new StandardRule(pNum: pNum, winLimit: 2, correctLimit: 10, wrongLimit: 10, consecutiveFlag: false);
        else if (rule.Item1 == "10divide10")
            game = new Divide(pNum: pNum, winLimit: 2);
        else if (rule.Item1 == "10HitsComboV")
            game = new HitsComboV(pNum: pNum, winLimit: 2, correctLimit: 10);
        game.InitialUpdate();
        game.makeHistory();
        updateDisplay();

        // ルール表示を更新
        questionPanel.transform.Find("Canvas/Rule").gameObject.GetComponent<TextMeshProUGUI>().text = "3rd Round:  " + rule.Item1;

    }


    public override void saveResult()
    {
        int order = -1;
        for (int i = 0; i < pNum; i++)
        {
            if (game.scores[i].winFlag > 0)
            {
                p[i].result["3R"] = game.scores[i].winFlag;
                p[i].entry["SF"] = "Entry";
                order = rule.Item2;
                if (p[i].result["3R"] == 1)
                {
                    p[i].pictureIndex = 5 - order;
                    Debug.Log(5 - order);
                }
                else if (p[i].result["3R"] == 2)
                {
                    p[i].pictureIndex = 5 + order;
                    Debug.Log(5 + order);
                }
            }
            else
            {
                p[i].result["3R"] = 0;
                p[i].entry["Ex2"] = "Entry";
            }
        }
    }

    public override void updateDisplay()
    {
        // 変えないといけないのは正解数, 誤答数の表示&色と, バーと背景パネルの色
        TextMeshProUGUI mainTMP, subTMP;
        Renderer barRenderer, backPanelRenderer;
        for (int i = 0; i < pNum; i++)
        {
            mainTMP = scoreBoards[i].transform.Find("Canvas/Main").gameObject.GetComponent<TextMeshProUGUI>();
            subTMP = scoreBoards[i].transform.Find("Canvas/Sub").gameObject.GetComponent<TextMeshProUGUI>();
            barRenderer = scoreBoards[i].transform.Find("bar").gameObject.GetComponent<Renderer>();
            backPanelRenderer = namePlates[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>();

            if (game.scores[i].winFlag > 0)
            { // 勝ち抜け
                mainTMP.text = ToOrdinalNumber(game.scores[i].winFlag);
                mainTMP.color = MainControllerScript.orange;
                subTMP.color = MainControllerScript.orange;
                barRenderer.material.color = MainControllerScript.orange;
            }
            else if (game.scores[i].loseFlag)
            { // 負け
                mainTMP.text = "Lose";
                mainTMP.color = MainControllerScript.gray;
                subTMP.color = MainControllerScript.gray;
                barRenderer.material.color = MainControllerScript.gray;
                backPanelRenderer.material.color = MainControllerScript.gray;
            }
            else
            {
                mainTMP.text = Convert.ToString(game.scores[i].point);
                mainTMP.color = MainControllerScript.white;
                subTMP.color = MainControllerScript.white;
                barRenderer.material.color = MainControllerScript.white;
                backPanelRenderer.material.color = MainControllerScript.plateColors[p[i].color];
            }
            if (rule.Item1 == "10up-down")
                subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 2);
            else if (rule.Item1 == "Freeze10")
                subTMP.text = game.scores[i].wrongNum.ToString() + "×";
            else if (rule.Item1 == "Swedish10")
                subTMP.text = ToDisplayStyleForSwedish10(game.scores[i].correctNum, game.scores[i].wrongNum);
            else if (rule.Item1 == "10by10")
                subTMP.text = game.scores[i].correctNum.ToString() + "×" + (10 - game.scores[i].wrongNum).ToString();
            else if (rule.Item1 == "10○10×")
                subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 10);
            else if (rule.Item1 == "10divide10")
                subTMP.text = game.scores[i].wrongNum.ToString() + "×";
            else if (rule.Item1 == "10HitsComboV")
                subTMP.text = game.scores[i].wrongNum.ToString() + "×";
        }
        if (rule.Item1 == "Freeze10" || rule.Item1 == "10HitsComboV")
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

    void Update()
    {
        // 連答状態によってアニメーションを変える
        if (rule.Item1 == "10HitsComboV" && consecutiveAnswerAnimatorFlag)
        {
            for (int i = 0; i < pNum; i++)
            {
                if (game.scores[i].consecutiveAnswersNum > 0 && game.scores[i].winFlag == 0)
                    scoreBoardAnimators[i].SetBool("Flag", true);
                else
                    scoreBoardAnimators[i].SetBool("Flag", false);
            }
            consecutiveAnswerAnimatorFlag = false;
        }
    }

    private string ToDisplayStyleForSwedish10(int cNum, int wNum)
    {
        int num1, num2; // ‐の数と・の数
        if (cNum == 0)
            num1 = 1;
        else if (cNum <= 2)
            num1 = 2;
        else if (cNum <= 5)
            num1 = 3;
        else
            num1 = 4;
        num1 = num1 <= 10 - wNum ? num1 : 10 - wNum;
        num2 = 10 - wNum - num1;
        string tmp = new string('×', wNum) + new string('-', num1) + new string('・', num2);
        return tmp.Insert(5, "\n");
    }
}




