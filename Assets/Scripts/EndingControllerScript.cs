using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingControllerScript : MonoBehaviour
{
    public GameObject panel;
    private int sceneCounter = 0;
    private GameObject[] panels;
    private Animator[] animators;

    private float timeLeft = 5f;

    void Start()
    {
        List<Player> p = MainControllerScript.players.GetRange(0, 100);

        panels = new GameObject[p.Count];
        animators = new Animator[p.Count];

        // プレートを生成
        for(int i = 0; i < p.Count; i++){
            panels[i] = Instantiate(panel, transform.position, transform.rotation); // 一旦画面中央に生成
            panels[i].transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            animators[i] = panels[i].GetComponent<Animator>();

            // プレイヤー毎の設定
            panels[i].transform.Find("Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            panels[i].transform.Find("Canvas/SubRank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            panels[i].transform.Find("Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].name; // 名前を入力
            panels[i].transform.Find("Canvas/SubRank").gameObject.GetComponent<TextMeshProUGUI>().color = MainControllerScript.plateColors[p[i].color]; // 順位の色変更
            panels[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更

            // 表示位置の調整
            if(i < 4)
                panels[i].transform.position += new Vector3((float)(-4.5+(i%4)*3.0), -0.2f, 0f);
            else
                panels[i].transform.position += new Vector3((float)(-4.5+(i%4)*3.0), (float)(-2+((i%8)/4)*3.6), 0f);
        }
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0 && sceneCounter < 13){
            timeLeft = 20f;
            if(sceneCounter == 0){
                for(int i = 0; i < 4; i++)
                    animators[sceneCounter*10+i].SetTrigger("Trigger2");
            }
            else{
                for(int i = 0; i < 8; i++)
                    animators[sceneCounter*8-4+i].SetTrigger("Trigger2");
            }
            sceneCounter++;     
        }
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
