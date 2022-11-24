using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControllerForExR1Script : MonoBehaviour
{
    public GameObject questionIndex;
    public GameObject question;
    public GameObject answer;
    public GameObject subAnswer;

    private int sceneCounter = 0;
    private List<Question> extraQuestions = new List<Question>(); // 問題データを格納

    private List<string> subAnswers = new List<string>();

    public TMP_InputField inputField;

    void Start()
    {
        using (StreamReader sr = new StreamReader("./Assets/Data/ExtraQuestions.csv"))
        {
            string line;
            string[] values;

            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                values = line.Split(',');
                extraQuestions.Add(new Question(values[1], values[2]));
                if (values.Length == 3)
                {
                    subAnswers.Add("");
                }
                else if (values.Length == 4)
                {
                    subAnswers.Add(values[3]);
                }
                else
                {
                    subAnswers.Add(values[3] + "\n" + values[4]);
                }
            }
        }
    }
    public void NextIsClicked()
    {

        if (sceneCounter < 30)
        {
            switch (sceneCounter % 3)
            {
                case 0:
                    questionIndex.GetComponent<TextMeshProUGUI>().text = "第" + (sceneCounter / 3 + 1).ToString() + "問";
                    questionIndex.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 1:
                    question.GetComponent<TextMeshProUGUI>().text = extraQuestions[sceneCounter / 3].question;
                    question.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 2:
                    questionIndex.GetComponent<Animator>().SetTrigger("Trigger");
                    question.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
            }
        }
        else if (sceneCounter < 50)
        {
            switch (sceneCounter % 2)
            {
                case 0:
                    answer.GetComponent<TextMeshProUGUI>().text = extraQuestions[(sceneCounter - 30) / 2].answer;
                    subAnswer.GetComponent<TextMeshProUGUI>().text = subAnswers[(sceneCounter - 30) / 2];
                    answer.GetComponent<Animator>().SetTrigger("Trigger");
                    subAnswer.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 1:
                    answer.GetComponent<Animator>().SetTrigger("Trigger");
                    subAnswer.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
            }
        }
        else if (sceneCounter < 75)
        {
            switch (sceneCounter % 5)
            {
                case 0:
                    questionIndex.GetComponent<TextMeshProUGUI>().text = "第" + (sceneCounter / 5 + 1).ToString() + "問";
                    questionIndex.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 1:
                    question.GetComponent<TextMeshProUGUI>().text = extraQuestions[sceneCounter / 5].question;
                    question.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 2:
                    questionIndex.GetComponent<Animator>().SetTrigger("Trigger");
                    question.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 3:
                    answer.GetComponent<TextMeshProUGUI>().text = extraQuestions[sceneCounter / 5].answer;
                    subAnswer.GetComponent<TextMeshProUGUI>().text = subAnswers[sceneCounter / 5];
                    answer.GetComponent<Animator>().SetTrigger("Trigger");
                    subAnswer.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
                case 4:
                    answer.GetComponent<Animator>().SetTrigger("Trigger");
                    subAnswer.GetComponent<Animator>().SetTrigger("Trigger");
                    break;
            }
        }
        sceneCounter++;
    }
    public void OnValueChanged()
    {
        string[] array = inputField.text.Split(','); // ペーパー順位
        int[] ranks = new int[8];
        int pNum = MainControllerScript.players.Count; // 全参加者数
        List<Player> p; // 参加プレイヤーのリスト

        p = MainControllerScript.players.FindAll(obj => obj.entry["Ex2"] == "Entry");
        if (0 < array.Length && array.Length <= 8 && p.Count == 40)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (!int.TryParse(array[i], out ranks[i])) return; // 正しいデータでなければやり直し
                if (ranks[i] > pNum) return;
            }
            for (int i = 0; i < array.Length; i++)
            {
                MainControllerScript.players[ranks[i] - 1].result["Ex1"] = 1;
                MainControllerScript.players[ranks[i] - 1].entry["Ex2"] = "Entry"; // playersはペーパー順位で並んでいる
                p.Add(MainControllerScript.players[ranks[i] - 1]);
            }
            p = p.OrderBy(a => Guid.NewGuid()).ToList();
            for (int i = 0; i < p.Count; i++)
            {
                p[i].entry["Ex2"] = "Group" + (i % 6 + 1).ToString();
            }
            inputField.text = "";
        }
    }
}
