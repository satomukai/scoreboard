using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerFor2RScript : GameControllerScript
{
    private int group; // 何組目か
    private Animator[] scoreBoardAnimators; // 連答エフェクトに関するScoreBoardsのAnimatorを管理

    void Start()
    {
        group = GroupSelectorFor2RScript.group; // グループ番号の取得

        base.Initialize("2R", "Group" + group);

        // プレートとスコアボードの表示位置を調整
        for (int i = 0; i < pNum; i++)
        {
            namePlates[i].transform.position += new Vector3(-6.05f + (float)(1.1 * i), -0.4f, 0f);
            scoreBoards[i].transform.position += new Vector3(-6.05f + (float)(1.1 * i), -3f, 0f);
        }

        // ルール特有のオブジェクトの設定
        scoreBoardAnimators = new Animator[pNum];
        for (int i = 0; i < pNum; i++)
            scoreBoardAnimators[i] = scoreBoards[i].GetComponent<Animator>();

        // ゲーム管理クラスのインスタンスを生成
        game = new StandardRule(pNum: pNum, winLimit: 5, correctLimit: 5, wrongLimit: 2, consecutiveFlag: true); // 連答付き5o2x(12->5)
        for (int i = 0; i < pNum; i++)
        {
            if (i == 0)
                game.scores[i] = new Score(correctNum: 3, wrongNum: 0);
            else if (i <= 2)
                game.scores[i] = new Score(correctNum: 2, wrongNum: 0);
            else if (i <= 5)
                game.scores[i] = new Score(correctNum: 1, wrongNum: 0);
            else
                game.scores[i] = new Score(correctNum: 0, wrongNum: 0);
        }
        game.InitialUpdate(); // アドバンテージを初期スコアに反映
        game.makeHistory();
        updateDisplay(); // スコア表示を更新
    }

    public override void saveResult()
    {
        for (int i = 0; i < pNum; i++)
        {
            if (game.scores[i].winFlag > 0)
                p[i].result["2R"] = game.scores[i].winFlag;
            else
            {
                p[i].result["2R"] = 0;
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
            subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 2);
        }
    }

    void Update()
    {
        // 連答状態によってアニメーションを変える
        if (animationFlag)
        {
            for (int i = 0; i < pNum; i++)
            {
                if (game.scores[i].consecutiveAnswersNum > 0 && game.scores[i].winFlag == 0)
                    scoreBoardAnimators[i].SetBool("Flag", true);
                else
                    scoreBoardAnimators[i].SetBool("Flag", false);
            }
            animationFlag = false;
        }
    }

}




