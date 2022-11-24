using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipButtonScript : MonoBehaviour
{
    public GameControllerScript gameControllerScript;

    public void OnClicked() {
        gameControllerScript.OnClicked(-1, false);
    }
}

