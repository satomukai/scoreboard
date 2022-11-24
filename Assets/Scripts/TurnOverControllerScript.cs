using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnOverControllerScript : MonoBehaviour
{

    public GroupSelectorScript groupSelectorScript;
    public GameObject panel;
    public TMP_InputField inputField;

    private int group;
    private int sceneCounter;
    private bool animationFlag;
    private GameObject[] panels;
    private Animator[] animators;

    // Update()でしか使わない
    private static int[] plateSum = {0, 1, 3, 6, 12}; // 以前に表示したプレートの累積枚数

    void Start()
    {
        group = GroupSelectorScript.group;
        sceneCounter = 0;
        animationFlag = false;

        List<Player> p = new List<Player>();
        if(1 <= group && group <= 4)
            p = MainControllerScript.players.FindAll(obj => obj.entry["2R"] == "Group"+group); // プレイヤー情報を取得(12人)
        else if(group == 5)
            p = MainControllerScript.players.GetRange(48, 1); // プレイヤー情報を取得(1人)

        panels = new GameObject[p.Count];
        animators = new Animator[p.Count];

        // プレートを生成
        for(int i = 0; i < p.Count; i++){
            panels[i] = Instantiate(panel, transform.position + new Vector3(0f, -0.2f, 0f), transform.rotation); // 一旦画面中央に生成
            animators[i] = panels[i].GetComponent<Animator>();

            // プレイヤー毎の設定
            panels[i].transform.Find("Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            panels[i].transform.Find("Canvas/SubRank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            panels[i].transform.Find("Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            panels[i].transform.Find("Canvas/SubRank").gameObject.GetComponent<TextMeshProUGUI>().color = MainControllerScript.plateColors[p[i].color]; // 順位の色変更
            panels[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更

            // 表示位置の調整(赤とピンクはそのまま中央に表示)
            if(1 <= i && i < 3) // 青プレ
                panels[i].transform.position += new Vector3((float)((i-1.5)*5.0), 0f, 0f);
            else if(3 <= i && i < 6) // 黄プレ
                panels[i].transform.position += new Vector3((float)((i-4.0)*4.0), 0f, 0f);
            else if(6 <= i && i < 12)// 緑プレ
                panels[i].transform.position += new Vector3((float)((i%3-1)*4.0), (float)(1.8+(2-i/3)*3.6), 0f);
            
        }
    }

    void Update()
    {
        // sceneCounterの値が増える度にアニメーションを進める(scene数はグループ1-3が11個, グループ4が12個)
        // 基本的に, "画面にプレート表示","ターンオーバー","フェードアウト"の3つを繰り返す
        // scene12以降, グループ1-3とグループ4で処理が分岐
        if(animationFlag && group < 5){
            int plate = (sceneCounter-1)/3; // 表示すべき色に対応したインデックス
            if(sceneCounter == 12 && group == 4){
                animators[11].SetTrigger("Trigger"); // 48位のプレートをターンオーバー
            }
            else if(sceneCounter == 11 && group == 4){
                for(int i = 6; i < 11; i++)
                    animators[i].SetTrigger("Trigger"); // 48位のプレートはターンオーバーしない
            }
            else if(sceneCounter < 12){ // これが基本の処理 上２つの分岐は例外
                for(int i = plateSum[plate]; i < plateSum[plate+1]; i++) 
                    animators[i].SetTrigger("Trigger"); // 対応するプレートのアニメーションをまとめて進める
            }
            animationFlag = false; // 次にsceneCounterの値が増えるまで待機
        }

        // 49thのターンオーバー
        if(animationFlag && group == 5){
            animators[0].SetTrigger("Trigger");
            animationFlag = false;
        }
    }

    public void SetQuestionIndex(){
        int qi;
        if(!int.TryParse(inputField.text, out qi)) return; // 正しいデータでなければやり直し
        MainControllerScript.questionIndex = (qi-1) % MainControllerScript.allQuestionNum;
        inputField.text = "";
    }

    // Nextボタンが押されたとき
    public void OnClicked()
    {
        sceneCounter++;
        animationFlag = true;
    }

    private string ToOrdinalNumber(int i){
        if(11 <= i && i <= 13)
            return i+"th";
        if(i % 10 == 1)
            return i+"st";
        else if (i % 10 == 2)
            return i+"nd";
        else if (i % 10 == 3)
            return i+"rd";
        else 
            return i+"th";
    }

}
