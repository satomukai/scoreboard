using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MainControllerScript : MonoBehaviour
{
    public static List<Player> players = new List<Player>(); // 参加者データを格納
    public static List<Question> questions = new List<Question>(); // 問題データを格納
    public static int questionIndex = 0; // 問題番号(0-index)
    public static int allQuestionNum = 0; // 全問題数
    public static (string, int)[] rules_3R = { ("10○10×", -1), ("10divide10", -1), ("Swedish10", -1), ("10HitsComboV", -1) }; // 適宜設定する
    public static Color32[] plateColors = new Color32[7] { new Color32(191, 144, 0, 255), new Color32(191, 191, 191, 255), new Color32(220, 40, 60, 255), new Color32(100, 240, 255, 255), new Color32(255, 215, 20, 255), new Color32(50, 255, 105, 255), new Color32(200, 100, 200, 255) };  // 各プレートの色
    public static Color32 white = new Color32(255, 255, 255, 255);
    public static Color32 orange = new Color32(255, 128, 80, 255); // 勝ち抜け
    public static Color32 gray = new Color32(128, 128, 128, 255); // 負け
    public static Color32 skyBlue = new Color32(80, 170, 170, 255); // 通常

    private static bool inputFlag = false; // 参加者データの読み込みが終わったらTrue

    public void inputData()
    {
        if (!inputFlag)
        {
            // 問題の読み込み
            using (StreamReader sr = new StreamReader("./Assets/Data/Questions.csv"))
            {
                string line;
                string[] values;

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    values = line.Split(',');
                    questions.Add(new Question(values[1], values[2]));
                }
                allQuestionNum = questions.Count;
            }

            // 参加者データ&スクリプトで使うデータの読み込み
            using (StreamReader sr = new StreamReader("./Assets/Data/PlayersList.csv"))
            {
                string line;
                string[] values;
                char[] del = { ',', '\t' };
                Player p;

                // 各種データの読込
                line = sr.ReadLine();
                values = line.Split(del);
                bool isInitial = values[1] == "1" ? true : false;
                if (!isInitial)
                {
                    questionIndex = (int.Parse(values[3]) - 1) % allQuestionNum;
                    rules_3R[0].Item2 = int.Parse(values[5]);
                    rules_3R[1].Item2 = int.Parse(values[6]);
                    rules_3R[2].Item2 = int.Parse(values[7]);
                    rules_3R[3].Item2 = int.Parse(values[8]);
                }

                line = sr.ReadLine(); // 1行とばす
                // プレイヤーデータの読み込み
                if (isInitial)
                { // 最初の読込のとき
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        values = line.Split(del);
                        p = new Player(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], -1);
                        players.Add(p);
                    }
                }
                else
                { // 2回目以降は結果も読み込む
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        values = line.Split(del);
                        p = new Player(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], int.Parse(values[5]));
                        p.entry["2R"] = values[6];
                        p.entry["3R"] = values[7];
                        p.entry["Ex1"] = values[8];
                        p.entry["Ex2"] = values[9];
                        p.entry["Ex3"] = values[10];
                        p.entry["SF"] = values[11];
                        p.entry["F"] = values[12];
                        p.result["1R"] = int.Parse(values[13]);
                        p.result["2R"] = int.Parse(values[14]);
                        p.result["3R"] = int.Parse(values[15]);
                        p.result["Ex1"] = int.Parse(values[16]);
                        p.result["Ex2"] = int.Parse(values[17]);
                        p.result["Ex3"] = int.Parse(values[18]);
                        p.result["SF"] = int.Parse(values[19]);
                        p.result["F"] = int.Parse(values[20]);
                        players.Add(p);
                    }
                }

                players.Sort((a, b) => a.paperRank - b.paperRank); // ペーパー順位で並び替え

                // ペーパー通過/未通過の処理 
                if (isInitial)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (i < 48)
                        {
                            players[i].result["1R"] = 1; // 通過
                            players[i].entry["2R"] = "Group" + (i % 4 + 1).ToString(); // 2Rへ参加
                        }
                        else
                        {
                            players[i].result["1R"] = 0; // 未通過
                            players[i].entry["Ex1"] = "Entry"; // 敗者復活1stStepにエントリー
                            players[i].result["Ex1"] = 0;
                        }
                    }
                }

                // 表示
                for (int i = 0; i < players.Count; i++)
                    Debug.Log(players[i].entryNumber + ", " + players[i].paperRank + ", " + players[i].name);
                for (int i = 0; i < questions.Count; i++)
                    Debug.Log(questions[i].question + ", " + questions[i].answer);
            }

            inputFlag = true; // 入力は1度だけ
        }
    }

    public void SaveData()
    {
        if (inputFlag)
        {
            using (StreamWriter sw = new StreamWriter("./Assets/Data/PlayersList.csv", false, encoding: Encoding.UTF8))
            {
                sw.WriteLine("最初の読込か,0,questionIndex," + (questionIndex + 1).ToString() + ",RuleOrder," + rules_3R[0].Item2 + "," + rules_3R[1].Item2 + "," + rules_3R[2].Item2 + "," + rules_3R[3].Item2);
                sw.WriteLine("管理番号,ペーパー順位,名前,所属,学年,PictureIndex,2R,3R,Ex1,Ex2,Ex3,SF,F,1R,2R,3R,Ex1,Ex2,Ex3,SF,F");
                for (int i = 0; i < players.Count; i++)
                {
                    sw.WriteLine(players[i].entryNumber + "," + players[i].paperRank + "," + players[i].name + "," + players[i].school + "," + players[i].grade + "," + players[i].pictureIndex + "," + players[i].entry["2R"] + "," + players[i].entry["3R"] + "," + players[i].entry["Ex1"] + "," + players[i].entry["Ex2"] + "," + players[i].entry["Ex3"] + "," + players[i].entry["SF"] + "," + players[i].entry["F"] + "," + players[i].result["1R"] + "," + players[i].result["2R"] + "," + players[i].result["3R"] + "," + players[i].result["Ex1"] + "," + players[i].result["Ex2"] + "," + players[i].result["Ex3"] + "," + players[i].result["SF"] + "," + players[i].result["F"]);
                }
            }
        }
    }
}

public class Player
{
    public int entryNumber;
    public int paperRank;
    public string name;
    public string school;
    public string grade;
    public int pictureIndex;
    public int color; // プレートの色に対応したインデックス(0:赤, 1:青, 2:黄, 3:緑, 4:ピンク)
    public Dictionary<string, string> entry; // 各ラウンドの参加状態を格納する
    public Dictionary<string, int> result; // 各ラウンドの結果を格納する(-1:未処理, 0:敗退, 1以上:勝ち抜け)

    public Player(int entryNumber, int paperRank, string name, string school, string grade, int pictureIndex)
    {
        this.entryNumber = entryNumber;
        this.paperRank = paperRank;
        this.name = name;
        this.school = school;
        this.grade = grade;
        this.pictureIndex = pictureIndex;
        this.entry = new Dictionary<string, string>() { { "2R", "" }, { "3R", "" }, { "Ex1", "" }, { "Ex2", "" }, { "Ex3", "" }, { "SF", "" }, { "F", "" } };
        this.result = new Dictionary<string, int>() { { "1R", -1 }, { "2R", -1 }, { "3R", -1 }, { "Ex1", -1 }, { "Ex2", -1 }, { "Ex3", -1 }, { "SF", -1 }, { "F", -1 } };
        // ペーパー順位に応じてプレートの色を決定
        if (paperRank == 1) this.color = 0;
        else if (paperRank <= 4) this.color = 1;
        else if (paperRank <= 12) this.color = 2;
        else if (paperRank <= 24) this.color = 3;
        else if (paperRank <= 48) this.color = 5;
        else this.color = 6;
    }
}

public class Question
{
    public string question;
    public string answer;

    public Question(string question, string answer)
    {
        this.question = question;
        this.answer = answer;
    }
}