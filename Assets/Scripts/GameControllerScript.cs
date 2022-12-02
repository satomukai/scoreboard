using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// 各ラウンドを管理するクラスは全てこのクラスを継承する
public abstract class GameControllerScript : MonoBehaviour
{
    public GameObject namePlate;
    public GameObject scoreBoard; // スコアボードは必ずCorrectButtonとWrongButtonを持つが, 表示部分がラウンドによって違う
    public GameObject questionPanel; // 問題パネルを管理 
    public GameObject timerPanel; // タイマーパネルを管理
    public GameObject questionCheck; // 次に出る問題をチェックする窓
    public TMP_InputField inputField; // 問題番号入力用
    protected bool updateFlag = true; // Trueのときだけ値が更新できる
    protected int backFlag = 0; // 直前の操作が巻き戻しだったからどうか 問題パネルを余計に回転させないために使う
    protected bool skipFlag = false; // 問題を読まずターンを進めるか
    protected bool animationFlag = true; // Trueのときだけ連答エフェクトの状態をを更新できる
    protected int pNum = 12; // 1ルールの参加者数
    protected List<Player> p; // 参加プレイヤーのリスト
    protected GameObject[] namePlates;
    protected GameObject[] scoreBoards;
    protected Game game;

    protected void Initialize(string key, string value)
    {
        Entry(key, value); // players.entry[key] == value のプレイヤーが参加
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
            namePlates[i].transform.Find("Canvas/Name").gameObject.GetComponent<TextMeshProUGUI>().text = ConvertVertical(p[i].name); // 名前を入力
            namePlates[i].transform.Find("Canvas/Rank").gameObject.GetComponent<TextMeshProUGUI>().text = ToOrdinalNumber(p[i].paperRank); // 順位を入力
            namePlates[i].transform.Find("Canvas/School").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].school; // 所属を入力
            namePlates[i].transform.Find("Canvas/Grade").gameObject.GetComponent<TextMeshProUGUI>().text = p[i].grade; // 学年を入力
            namePlates[i].transform.Find("BackPanel").gameObject.GetComponent<Renderer>().material.color = MainControllerScript.plateColors[p[i].color]; // 背景パネルの色変更           

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
    }

    // players.entryを見てセットの参加者を登録する 人が足りないときは適当に登録する(デバッグ用) 大会によって変える必要がある
    protected void Entry(string key, string value)
    {
        int count = MainControllerScript.players.Count(obj => obj.entry[key] == value); // ルールの参加人数をチェック
        if (key == "2R")
        {
            if (count == 12)
                p = MainControllerScript.players.FindAll(obj => obj.entry[key] == value);
            else
                p = MainControllerScript.players.GetRange(0, 12);
        }
        else if (key == "3R")
        {
            if (count == 5)
            {
                p = MainControllerScript.players.FindAll(obj => obj.entry[key] == value);
                p.Sort((a, b) => a.result["2R"] - b.result["2R"]);
            }
            else
                p = MainControllerScript.players.GetRange(0, 5);
        }
        else if (key == "Ex2")
        {
            if (count >= 6)
                p = MainControllerScript.players.FindAll(obj => obj.entry[key] == value);
            else
                p = MainControllerScript.players.GetRange(0, 8);
        }
        else if (key == "Ex3")
        {
            if (count == 12)
                p = MainControllerScript.players.FindAll(obj => obj.entry[key] == value);
            else
                p = MainControllerScript.players.GetRange(0, 12);
        }
        else if (key == "F")
        {
            if (count > 0)
            {
                p = MainControllerScript.players.FindAll(obj => obj.entry[key] == value);
                p.Sort((a, b) => a.result["SF"] - b.result["SF"]);
            }
            else
                p = MainControllerScript.players.GetRange(0, 3);
        }
    }

    // 正解,誤答,スルー, スキップのボタンが押された(スルーのときはindex:-1, correct:true, スキップのときはindex:-1, correct:false)
    public virtual void OnClicked(int index, bool correct)
    {
        if (updateFlag)
        {
            updateFlag = !game.Update(index, correct); // スコアの更新(Trueが返ってきたらupdateFlagはFalseとし, Nextが押されるのを待つ)
            if (index == -1 && !correct) // スキップが押された
                skipFlag = true;
            else
                skipFlag = false;
            // スコアが正しく更新されたとき
            if (!updateFlag)
            {
                updateDisplay();
                animationFlag = true;
            }
            if (game.timerStopFlag && timerPanel != null)
                timerPanel.transform.Find("Canvas1/TimerText").gameObject.GetComponent<TimerScript>().StopTimer();
        }
    }

    // Nextボタンが押された
    public virtual void NextIsClicked()
    {
        if (!updateFlag)
        {
            if (backFlag > 0)
            {
                backFlag--;
            }
            else if (!skipFlag)
            { // 直前の操作がキップのときは回転しないようにしておく
                questionPanel.transform.Find("Canvas/Question").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[MainControllerScript.questionIndex].question;
                questionPanel.transform.Find("Canvas/Answer").gameObject.GetComponent<TextMeshProUGUI>().text = MainControllerScript.questions[MainControllerScript.questionIndex].answer;
                questionPanel.GetComponent<Animator>().SetTrigger("Trigger");
                MainControllerScript.questionIndex = ++MainControllerScript.questionIndex % MainControllerScript.allQuestionNum; // 問題番号を1進める
                questionCheck.GetComponent<TextMeshProUGUI>().text = "次の問題: " + MainControllerScript.questions[MainControllerScript.questionIndex].question.Substring(0, 10) + "...";
            }
            updateFlag = true; // 得点の更新あるいは遡及を1度行うと, Nextが押されるまでそれ以上変更できない
        }
    }

    // Backボタンが押された
    public virtual void BackIsClicked()
    {
        if (updateFlag)
        {
            game.Back(); // 巻き戻し処理を行う
            updateDisplay(); // 表示を更新
            backFlag++;
            animationFlag = true;
        }
    }

    // Endボタンが押された
    public virtual void EndIsClicked()
    {
        if (updateFlag)
        {
            game.End(); // 勝ち抜け者を決める
            updateDisplay(); // 表示を更新
            saveResult(); // 結果をプレイヤーリストに反映
            animationFlag = true;
            updateFlag = false;
        }
    }

    // 不測の事態のときに問題番号を設定する
    public void SetQuestionIndex()
    {
        int qi;
        if (!int.TryParse(inputField.text, out qi)) return; // 正しいデータでなければやり直し
        MainControllerScript.questionIndex = (qi - 1) % MainControllerScript.allQuestionNum;
        questionCheck.GetComponent<TextMeshProUGUI>().text = "次の問題: " + MainControllerScript.questions[MainControllerScript.questionIndex].question.Substring(0, 10) + "...";
        inputField.text = "";
    }

    // 対戦結果をゲームに反映させる
    public abstract void saveResult();

    // スコアが変わったときに呼ばれ, 画面の表示を更新する
    public abstract void updateDisplay();

    // 整数を受け取り序数詞にして返す
    protected string ToOrdinalNumber(int i)
    {
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

    // 見た目上縦書きにするために文字の間に改行を入れる
    private string ConvertVertical(string s)
    {
        int len = s.Length;
        string tmp = "";
        for (int i = 0; i < 2 * len - 1; i++)
        {
            if (i % 2 == 0) tmp += s[i / 2];
            else tmp += '\n';
        }
        return tmp;
    }

    // 誤答数を表示用の形式にして返す(ex. 3x失格のルールで誤答数2->"xx-")
    protected string ToDisplayStyle(int wNum, int wLimit, bool foldingFlag = true)
    {
        if (wLimit == -1)
            return "";
        else
        {
            string tmp = new string('×', wNum) + new string('・', wLimit - wNum);
            if (wLimit > 5 && foldingFlag)
                return tmp.Insert(5, "\n");
            else
                return tmp;
        }
    }

}


// -------------------ゲームに関係するクラスの定義------------------
// ゲーム管理
public abstract class Game
{
    public Score[] scores;
    public int winLimit; // 勝ち抜け枠
    public int winNum; // 勝ち抜け人数

    public bool timerStopFlag; // trueになったときタイマーを止める
    protected List<History> history; // 履歴

    public Game(int pNum, int winLimit)
    {
        this.scores = new Score[pNum];
        for (int i = 0; i < pNum; i++)
            this.scores[i] = new Score(correctNum: 0, wrongNum: 0);
        this.winLimit = winLimit;
        this.winNum = 0;
        this.timerStopFlag = false;
        this.history = new List<History>();
    }

    // アドバンテージに応じて初期スコアを更新 アドバンテージがないルールなら呼ばなくてよい
    public virtual void InitialUpdate() { }

    // スコアの更新
    public abstract bool Update(int index, bool correct);

    // 終了処理(基本は正解数の多さ->誤答の少なさ->ペーパー順位)
    public virtual void End()
    {
        Score winner = scores[0];
        while (winNum < winLimit)
        { // 残りの勝ち抜け者を選ぶ 
            int winnerIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i].winFlag == 0 && !scores[i].loseFlag)
                { // 未勝ち抜けかつ失格していない人から選ぶ
                    if (winnerIndex == -1)
                    {
                        winnerIndex = i;
                        winner = scores[i];
                    }
                    else
                    {
                        // 正解の多さ->誤答の少なさで判定
                        if (scores[i].correctNum > winner.correctNum || scores[i].correctNum == winner.correctNum && scores[i].wrongNum < winner.wrongNum)
                        {
                            winnerIndex = i;
                            winner = scores[i];
                        }
                    }
                }
            }
            scores[winnerIndex].winFlag = ++winNum;
        }
        makeHistory();
    }

    // 1つ前の状態に戻す
    public void Back()
    {
        if (history.Count > 1)
        { // 履歴が2つ以上あれば
            // historyの末尾には今の状態が記録されているのでそれより1つ前(index:history.Count-2)をコピー
            for (int i = 0; i < this.scores.Length; i++)
            {
                this.scores[i] = new Score(history[history.Count - 2].scores[i]);
            }
            this.winNum = history[history.Count - 2].winNum;
            history.RemoveAt(history.Count - 1); // historyの末尾を削除
        }
    }

    // 現在のGameの状態をhistoryに記録する
    public void makeHistory()
    {
        history.Add(new History(this));
    }

    // 記録管理用のクラス
    protected class History
    {
        public Score[] scores;
        public int[] freezeTime;
        public int winNum; // 勝ち抜け人数

        // Gameから必要な情報をコピーする
        internal History(Game game)
        {
            this.scores = new Score[game.scores.Length];
            for (int i = 0; i < this.scores.Length; i++)
                this.scores[i] = new Score(game.scores[i]);
            this.winNum = game.winNum;
        }
    }

}

// スコア管理
public class Score
{
    public int correctNum; // 正解数
    public int wrongNum; //　誤答数
    public int point; // 表示する得点
    public int winFlag; // 勝ち抜けた順に1,2,...
    public bool loseFlag; // 負けフラグ
    public int consecutiveAnswersNum; //  連答数
    public int freezeTime; //　 休みの数

    public Score(Score source)
    {
        this.correctNum = source.correctNum;
        this.wrongNum = source.wrongNum;
        this.point = source.point;
        this.winFlag = source.winFlag;
        this.loseFlag = source.loseFlag;
        this.consecutiveAnswersNum = source.consecutiveAnswersNum;
        this.freezeTime = source.freezeTime;
    }

    public Score(int correctNum, int wrongNum)
    {
        this.correctNum = correctNum;
        this.wrongNum = wrongNum;
        this.point = 0;
        this.winFlag = 0;
        this.loseFlag = false;
        this.consecutiveAnswersNum = 0;
        this.freezeTime = 0;
    }

}


// --------------各種ルール--------------
// MoNx
public class StandardRule : Game
{
    private int correctLimit; // 必要正解数
    private int wrongLimit; // 許容誤答数
    private bool consecutiveFlag; // 連答付きならTrue
    private int freezeNum = 0; // 休みの数

    public StandardRule(int pNum, int winLimit, int correctLimit, int wrongLimit, bool consecutiveFlag = false, int freezeNum = 0) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
        this.wrongLimit = wrongLimit;
        this.consecutiveFlag = consecutiveFlag;
        this.freezeNum = freezeNum;
    }

    public override void InitialUpdate()
    {
        for (int i = 0; i < scores.Length; i++)
            scores[i].point = scores[i].correctNum;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag && scores[index].freezeTime == 0)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum += scores[index].consecutiveAnswersNum + 1; // 連答数に応じて正解数を増やす
                scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される
                // 連答付きなら全プレイヤーの連答状態を変更
                if (consecutiveFlag)
                {
                    for (int i = 0; i < scores.Length; i++)
                    {
                        if (i != index)
                            scores[i].consecutiveAnswersNum = 0;
                        else
                            scores[i].consecutiveAnswersNum = 1;
                    }
                }
            }
            else
            { // 不正解
                scores[index].wrongNum++;
                scores[index].freezeTime = freezeNum; // 休みを増やす
                scores[index].consecutiveAnswersNum = 0; // 誤答したら連答リセット
            }

            // 休みを減らす
            for (int i = 0; i < scores.Length; i++)
                if (i != index && scores[i].freezeTime > 0) scores[i].freezeTime--;

            // 勝ち抜け/負けの処理
            if (scores[index].correctNum >= correctLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (wrongLimit > 0 && scores[index].wrongNum >= wrongLimit)
            {
                timerStopFlag = true;
                scores[index].loseFlag = true;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            for (int i = 0; i < scores.Length; i++)
                if (scores[i].freezeTime > 0) scores[i].freezeTime--;
            makeHistory();
            return true;
        }
        return false;
    }
}

// By
public class By : Game
{
    private int n;
    private int wrongLimit; // 許容誤答数
    public By(int pNum, int winLimit, int n, int wrongLimit) : base(pNum, winLimit)
    {
        this.n = n;
        this.wrongLimit = wrongLimit;
    }

    public override void End()
    {
        Score winner = scores[0];
        while (winNum < winLimit)
        { // 残りの勝ち抜け者を選ぶ 
            int winnerIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i].winFlag == 0 && !scores[i].loseFlag)
                { // 未勝ち抜けかつ失格していない人から選ぶ
                    if (winnerIndex == -1)
                    {
                        winnerIndex = i;
                        winner = scores[i];
                    }
                    else
                    {
                        // ポイントの多さ->正解の多さで判定
                        if (scores[i].point > winner.point || scores[i].point == winner.point && scores[i].correctNum > winner.correctNum)
                        {
                            winnerIndex = i;
                            winner = scores[i];
                        }
                    }
                }
            }
            scores[winnerIndex].winFlag = ++winNum;
        }
        makeHistory();
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct) // 正解
                scores[index].correctNum++; // 連答数に応じて正解数を増やす
            else // 不正解
                scores[index].wrongNum++; // 連答数に応じて正解数を増やす

            scores[index].point = scores[index].correctNum * (n - scores[index].wrongNum); // 正解数×(n-誤答数)を表示

            // 勝ち抜け/負けの処理
            if (scores[index].point >= n * n)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (scores[index].wrongNum >= wrongLimit)
            {
                timerStopFlag = true;
                scores[index].loseFlag = true;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// up-down
public class UpDown : Game
{
    private int correctLimit;
    public UpDown(int pNum, int winLimit, int correctLimit) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum++; // 連答数に応じて正解数を増やす
            }
            else
            { // 不正解
                scores[index].wrongNum++;
                scores[index].correctNum = 0;
            }

            scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される

            // 勝ち抜け/負けの処理
            if (scores[index].correctNum >= correctLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (scores[index].wrongNum >= 2)
            { // 2×で失格
                timerStopFlag = true;
                scores[index].loseFlag = true;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// Swedish10
public class Swedish10 : Game
{
    public Swedish10(int pNum, int winLimit) : base(pNum, winLimit)
    {
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum++; // 連答数に応じて正解数を増やす
                scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される
            }
            else
            { // 不正解
                if (scores[index].correctNum == 0)
                    scores[index].wrongNum += 1;
                else if (scores[index].correctNum <= 2)
                    scores[index].wrongNum += 2;
                else if (scores[index].correctNum <= 5)
                    scores[index].wrongNum += 3;
                else if (scores[index].correctNum <= 10)
                    scores[index].wrongNum += 4;
            }

            // 勝ち抜け/負けの処理
            if (scores[index].correctNum >= 10)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (scores[index].wrongNum >= 10)
            {
                timerStopFlag = true;
                scores[index].loseFlag = true;
                scores[index].wrongNum = 10;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// Freeze
public class Freeze : Game
{
    private int correctLimit; // 必要正解数
    public Freeze(int pNum, int winLimit, int correctLimit) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && scores[index].freezeTime == 0)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum++; // 正解数を増やす
                scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される
            }
            else
            { // 不正解
                scores[index].freezeTime = ++scores[index].wrongNum; // 誤答数分休みが増える
            }

            // 勝ち抜けの処理
            if (scores[index].correctNum >= correctLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }

            // 休みを減らす
            for (int i = 0; i < scores.Length; i++)
                if (i != index && scores[i].freezeTime > 0) scores[i].freezeTime--;

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            // 休みを減らす
            for (int i = 0; i < scores.Length; i++)
                if (scores[i].freezeTime > 0) scores[i].freezeTime--;

            makeHistory();
            return true;
        }
        return false;
    }
}

// 10divide10
public class Divide : Game
{
    private int correctLimit;
    public Divide(int pNum, int winLimit) : base(pNum, winLimit)
    {
        for (int i = 0; i < scores.Length; i++)
            scores[i].point = 10;
    }

    public override void End()
    {
        Score winner = scores[0];
        while (winNum < winLimit)
        { // 残りの勝ち抜け者を選ぶ 
            int winnerIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i].winFlag == 0 && !scores[i].loseFlag)
                { // 未勝ち抜けかつ失格していない人から選ぶ
                    if (winnerIndex == -1)
                    {
                        winnerIndex = i;
                        winner = scores[i];
                    }
                    else
                    {
                        // ポイントの多さ->正解の多さで判定
                        if (scores[i].point > winner.point)
                        {
                            winnerIndex = i;
                            winner = scores[i];
                        }
                    }
                }
            }
            scores[winnerIndex].winFlag = ++winNum;
        }
        makeHistory();
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum++;
                scores[index].point += 10;
            }
            else
            { // 不正解
                scores[index].wrongNum++;
                scores[index].point = (int)(scores[index].point / scores[index].wrongNum);
            }

            // 勝ち抜け/負けの処理
            if (scores[index].point >= 100)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (scores[index].point == 0)
            { //
                timerStopFlag = true;
                scores[index].loseFlag = true;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// 10HitsComboV
public class HitsComboV : Game
{
    private int correctLimit; // 必要正解数
    public HitsComboV(int pNum, int winLimit, int correctLimit) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && scores[index].freezeTime == 0)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum += scores[index].consecutiveAnswersNum; // 正解数を増やす
                scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される
                for (int i = 0; i < scores.Length; i++)
                {
                    if (i != index)
                        scores[i].consecutiveAnswersNum = 0;
                    else
                        scores[i].consecutiveAnswersNum++;
                }
            }
            else
            { // 不正解
                scores[index].wrongNum++;
                scores[index].freezeTime = scores[index].wrongNum; // 誤答数分休みが増える
                scores[index].consecutiveAnswersNum = 0;
            }

            // 勝ち抜けの処理
            if (scores[index].correctNum >= correctLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }

            // 休みを減らす
            for (int i = 0; i < scores.Length; i++)
                if (i != index && scores[i].freezeTime > 0) scores[i].freezeTime--;

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            // 休みを減らす
            for (int i = 0; i < scores.Length; i++)
                if (scores[i].freezeTime > 0) scores[i].freezeTime--;

            makeHistory();
            return true;
        }
        return false;
    }
}

// 2Combo-Hit Pass-Gate
public class PassGate : Game
{

    public PassGate(int pNum, int winLimit) : base(pNum, winLimit) { }

    public override void InitialUpdate()
    {
        for (int i = 0; i < scores.Length; i++)
            scores[i].point = scores[i].correctNum;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                switch (scores[index].point)
                {
                    case 0:
                        scores[index].point = 1;
                        break;
                    case 1:
                        bool blockFlag = false;
                        for (int i = 0; i < scores.Length; i++)
                        {
                            if (!scores[i].loseFlag && scores[i].point == 2)
                            {
                                scores[i].point = 1;
                                blockFlag = true;
                            }
                        }
                        if (!blockFlag)
                            scores[index].point = 2;
                        break;
                    case 2:
                        scores[index].point = 3;
                        break;
                }
            }
            else
            { // 不正解
                scores[index].wrongNum++;
            }

            // 勝ち抜け/負けの処理
            if (scores[index].point >= 3)
                scores[index].winFlag = ++winNum;
            if (scores[index].wrongNum >= 1)
                scores[index].loseFlag = true;

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// 変則 NoMx 誤答で必要正解数が2増える
public class IrregularRule : Game
{
    private int correctLimit; // 必要正解数
    private int wrongLimit; // 許容誤答数
    public IrregularRule(int pNum, int winLimit, int correctLimit, int wrongLimit) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
        this.wrongLimit = wrongLimit;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag && scores[index].freezeTime == 0)
        {
            // 正解数/誤答数の処理
            if (correct) // 正解
                scores[index].correctNum++; // 連答数に応じて正解数を増やす
            else  // 不正解
                scores[index].wrongNum++;

            scores[index].point = scores[index].correctNum;

            // 勝ち抜け/負けの処理
            if (scores[index].correctNum >= correctLimit + scores[index].wrongNum * 2)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (wrongLimit > 0 && scores[index].wrongNum >= wrongLimit)
            {
                timerStopFlag = true;
                scores[index].loseFlag = true;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}

// アタックサバイバル
public class AttackSurvival : Game
{
    private int hp;
    private int minusByOthersCorrectAnswer; // 必要正解数
    private int minusByMyWrongAnswer; // 許容誤答数
    public AttackSurvival(int pNum, int winLimit, int hp, int minusByOthersCorrectAnswer, int minusByMyWrongAnswer) : base(pNum, winLimit)
    {
        for (int i = 0; i < scores.Length; i++)
            scores[i].point = hp;
        this.hp = hp;
        this.minusByOthersCorrectAnswer = minusByOthersCorrectAnswer; // 他人の正解で減るHP
        this.minusByMyWrongAnswer = minusByMyWrongAnswer; // 自分の誤答で減るHP
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && !scores[index].loseFlag)
        {
            // 正解数/誤答数の処理
            if (correct) // 正解
                scores[index].correctNum++; // 連答数に応じて正解数を増やす
            else  // 不正解
                scores[index].wrongNum++;

            for (int i = 0; i < scores.Length; i++)
            {
                scores[i].point = hp;
                for (int j = 0; j < scores.Length; j++)
                {
                    if (i != j)
                        scores[i].point -= scores[j].correctNum * minusByOthersCorrectAnswer;
                    else
                        scores[i].point -= scores[i].wrongNum * minusByMyWrongAnswer;
                }
                // 負けの処理
                if (scores[i].point <= 0)
                {
                    scores[i].point = 0;
                    scores[i].loseFlag = true;
                }
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }

    public override void End()
    {
        Score winner = scores[0];
        while (winNum < winLimit)
        { // 残りの勝ち抜け者を選ぶ 
            int winnerIndex = -1;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i].winFlag == 0 && !scores[i].loseFlag)
                { // 未勝ち抜けかつ失格していない人から選ぶ
                    if (winnerIndex == -1)
                    {
                        winnerIndex = i;
                        winner = scores[i];
                    }
                    else
                    {
                        // 残り体力の多さで判定
                        if (scores[i].point > winner.point)
                        {
                            winnerIndex = i;
                            winner = scores[i];
                        }
                    }
                }
            }
            scores[winnerIndex].winFlag = ++winNum;
        }
        makeHistory();
    }
}

public class ComboShot : Game
{
    private int correctLimit; // 必要正解数
    private int wrongLimit; // 許容誤答数
    public ComboShot(int pNum, int winLimit, int correctLimit, int wrongLimit) : base(pNum, winLimit)
    {
        this.correctLimit = correctLimit;
        this.wrongLimit = wrongLimit;
    }

    public override bool Update(int index, bool correct)
    {
        timerStopFlag = false;
        if (0 <= index && index < scores.Length && scores[index].winFlag == 0 && scores[index].freezeTime == 0)
        {
            // 正解数/誤答数の処理
            if (correct)
            { // 正解
                scores[index].correctNum += scores[index].consecutiveAnswersNum; // 正解数を増やす
                scores[index].point = scores[index].correctNum; // このルールは正解数がそのまま表示される
                for (int i = 0; i < scores.Length; i++)
                {
                    if (i != index)
                        scores[i].consecutiveAnswersNum = 0;
                    else
                        scores[i].consecutiveAnswersNum++;
                }
            }
            else
            { // 不正解
                scores[index].wrongNum++;
                scores[index].consecutiveAnswersNum = 0;
            }

            // 勝ち抜け/負けの処理
            if (scores[index].correctNum >= correctLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }
            if (scores[index].wrongNum >= wrongLimit)
            {
                timerStopFlag = true;
                scores[index].winFlag = ++winNum;
            }

            makeHistory();
            return true;
        }
        else if (index == -1)
        { // スルーはindex:-1に対応
            makeHistory();
            return true;
        }
        return false;
    }
}


