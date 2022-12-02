using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForExR3Script : GameControllerScript
{
    private int group; // 何組目か
    private Animator[] scoreBoardAnimators; // ScoreBoardsのAnimatorを管理
    void Start()
    {
        base.Initialize("Ex3", "Entry");

        // プレートとスコアボードの表示位置を調整
        for (int i = 0; i < pNum; i++)
        {
            namePlates[i].transform.position += new Vector3((float)(-0.55 * (pNum - 1)) + (float)(1.1 * i), -0.4f, 0f);
            scoreBoards[i].transform.position += new Vector3((float)(-0.55 * (pNum - 1)) + (float)(1.1 * i), -3f, 0f);
        }

        // ルール特有のオブジェクトの設定
        scoreBoardAnimators = new Animator[pNum];
        for (int i = 0; i < pNum; i++)
            scoreBoardAnimators[i] = scoreBoards[i].GetComponent<Animator>();

        // ゲーム管理クラスのインスタンスを生成
        game = new PassGate(pNum: pNum, winLimit: 1); // 連答付き5o2x(12->5)
        game.makeHistory();
        updateDisplay(); // スコア表示を更新
    }

    public override void saveResult()
    {
        for (int i = 0; i < pNum; i++)
        {
            if (game.scores[i].winFlag > 0)
            {
                p[i].result["Ex3"] = 1;
                p[i].entry["SF"] = "Entry";
                p[i].pictureIndex = 5;
            }
            else
                p[i].result["Ex3"] = 0;
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
            subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 1);
        }
    }

    void Update()
    {
        // 連答状態によってアニメーションを変える
        if (animationFlag)
        {
            for (int i = 0; i < pNum; i++)
            {
                if (game.scores[i].point == 2 && game.scores[i].winFlag == 0 && !game.scores[i].loseFlag)
                    scoreBoardAnimators[i].SetBool("Flag", true);
                else
                    scoreBoardAnimators[i].SetBool("Flag", false);
            }
            animationFlag = false;
        }
    }
}




