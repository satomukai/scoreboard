using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForExR2Script : GameControllerScript {
    private int group; // 何組目か
    void Start() {
        group = GroupSelectorForExR2Script.group; // グループ番号の取得

        base.Initialize("Ex2", "Group" + group);

        // プレートとスコアボードの表示位置を調整
        for (int i = 0; i < pNum; i++) {
            namePlates[i].transform.position += new Vector3((float)(-0.55 * (pNum - 1)) + (float)(1.1 * i), -0.4f, 0f);
            scoreBoards[i].transform.position += new Vector3((float)(-0.55 * (pNum - 1)) + (float)(1.1 * i), -3f, 0f);
        }

        // ゲーム管理クラスのインスタンスを生成
        game = new StandardRule(pNum: pNum, winLimit: 2, correctLimit: 2, wrongLimit: 1);
        game.makeHistory();
        updateDisplay(); // スコア表示を更新
    }

    public override void saveResult() {
        for (int i = 0; i < pNum; i++) {
            if (game.scores[i].winFlag > 0) {
                p[i].result["Ex2"] = game.scores[i].winFlag;
                p[i].entry["Ex3"] = "Entry";
            } else
                p[i].result["Ex2"] = 0;
        }
    }

    public override void updateDisplay() {
        // 変えないといけないのは正解数, 誤答数の表示&色と, バーと背景パネルの色
        TextMeshProUGUI mainTMP, subTMP;
        Renderer barRenderer, backPanelRenderer;
        for (int i = 0; i < pNum; i++) {
            mainTMP = scoreBoards[i].transform.Find("Canvas/Main").gameObject.GetComponent<TextMeshProUGUI>();
            subTMP = scoreBoards[i].transform.Find("Canvas/Sub").gameObject.GetComponent<TextMeshProUGUI>();
            barRenderer = scoreBoards[i].transform.Find("bar").gameObject.GetComponent<Renderer>();
            backPanelRenderer = namePlates[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>();

            if (game.scores[i].winFlag > 0) { // 勝ち抜け
                mainTMP.text = ToOrdinalNumber(game.scores[i].winFlag);
                mainTMP.color = MainControllerScript.orange;
                subTMP.color = MainControllerScript.orange;
                barRenderer.material.color = MainControllerScript.orange;
            } else if (game.scores[i].loseFlag) { // 負け
                mainTMP.text = "Lose";
                mainTMP.color = MainControllerScript.gray;
                subTMP.color = MainControllerScript.gray;
                barRenderer.material.color = MainControllerScript.gray;
                backPanelRenderer.material.color = MainControllerScript.gray;
            } else {
                mainTMP.text = Convert.ToString(game.scores[i].point);
                mainTMP.color = MainControllerScript.white;
                subTMP.color = MainControllerScript.white;
                barRenderer.material.color = MainControllerScript.white;
                backPanelRenderer.material.color = MainControllerScript.plateColors[p[i].color];
            }
            subTMP.text = ToDisplayStyle(game.scores[i].wrongNum, 1);
        }
    }

    void Update() { }
}




