using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class TimerScript : MonoBehaviour {
	public TextMeshProUGUI timerText;
    public int setMinutes;
    public int setSeconds;

    private float setTime;
    private int minutes;
    private float seconds;   
    private bool activeFlag = false; // trueのときだけタイマーを動かす

	void Start () {
        setTime = (float)(setMinutes*60 + setSeconds);
        minutes = (int) setTime / 60;
        seconds = setTime - minutes * 60;
        refreshDisplay();
	}
 
	void Update () {
		if(activeFlag){
            setTime -= Time.deltaTime;
            minutes = (int) setTime / 60;
            seconds = setTime - minutes * 60;
            refreshDisplay();
            if(setTime <= 0) 
                activeFlag = false;
        }
	}

    // 表示を更新する
    private void refreshDisplay(){
        if(setTime <= 0)
            timerText.text = "0.00";
        else if(minutes == 0) // 残り1分を切っているとき表示を変える
            timerText.text = seconds.ToString("F2");
        else
            timerText.text = minutes.ToString("00") + ":" + ((int)seconds).ToString("00");
    }

    public void StartTimer(){
        activeFlag = true;
    }

    public void StopTimer(){
        activeFlag = false;
    }

    public void Reset(){
        activeFlag = false;
        setTime = (float)(setMinutes*60 + setSeconds);
        minutes = (int) setTime / 60;
        seconds = setTime - minutes * 60;
        refreshDisplay();
    }
}


// 実際の時刻を基にしたタイマー より正確?
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class TimerScript : MonoBehaviour
// {
//     public TextMeshProUGUI timerText;
//     public int setMinute;
//     public int setSecond;
//     private DateTime finishTime; // 終了時刻
//     private TimeSpan setTime; // 測る時間
//     private static TimeSpan oneMinute = new TimeSpan(0, 1, 0); // 1分を表す定数
//     private TimeSpan timer;

//     private bool activeFlag = false;

//     void Start()
//     {
//         setTime = new TimeSpan(0, setMinute, setSecond);
//         timerText.text = setTime.ToString(@"mm\:ss");
//         finishTime = DateTime.Now + (setTime + new TimeSpan(0, 0, 1));
//         timer = setTime;
//     }

//     void Update()
//     {
//         if(activeFlag){
//             if(TimeSpan.Compare(timer, oneMinute) == -1) // 残り時間1分を切ったら表示形式を変える
//                 timerText.text = timer.ToString(@"ss\.ff");
//             else
//                 timerText.text = timer.ToString(@"mm\:ss");
            

//             // countdown
//             if(TimeSpan.Compare(timer, TimeSpan.Zero) == 1)
//                 timer =  finishTime - DateTime.Now;
//             else
//                 timer = TimeSpan.Zero;
//         }
//     }

//     public void StartTimer(){
//         activeFlag = true;
//     }

//     public void StopTimer(){
//         activeFlag = false;
//     }
// }