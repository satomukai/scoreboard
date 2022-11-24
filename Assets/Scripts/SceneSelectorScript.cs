using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectorScript : MonoBehaviour
{
    public void OnValueChanged(int value)
    {
        switch (value)
        {
            case 1:
                SceneManager.LoadScene("Main");
                break;
            case 2:
                SceneManager.LoadScene("1R");
                break;
            case 3:
                SceneManager.LoadScene("TurnOver");
                break;
            case 4:
                SceneManager.LoadScene("2R");
                break;
            case 5:
                SceneManager.LoadScene("CourseChoice");
                break;
            case 6:
                SceneManager.LoadScene("3R");
                break;
            case 7:
                SceneManager.LoadScene("ExR1");
                break;
            case 8:
                SceneManager.LoadScene("ExR2");
                break;
            case 9:
                SceneManager.LoadScene("ExR3");
                break;
            case 10:
                SceneManager.LoadScene("SF");
                break;
            case 11:
                SceneManager.LoadScene("F");
                break;
            case 12:
                SceneManager.LoadScene("Ending");
                break;
            default:
                break;
        }
    }
}
