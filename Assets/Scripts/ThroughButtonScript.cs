using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThroughButtonScript : MonoBehaviour
{
    public GameControllerScript gameControllerScript;

    public void OnClicked() {
        gameControllerScript.OnClicked(-1, true);
    }
}
