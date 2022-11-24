using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetButtonScript : MonoBehaviour
{
    public ControllerForCourseChoiceScript controllerForCourseChoiceScript;
    public int index; // ボタンのインデックス

    public void OnClicked() {
        controllerForCourseChoiceScript.OnClicked(this.index);
    }
}
