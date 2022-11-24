using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerFor1RScript : MonoBehaviour
{
    public void StartStaffRoll()
    {
        this.GetComponent<Animator>().SetTrigger("StaffRoll");
    }

    public void SwitchPanels()
    {
        this.GetComponent<Animator>().SetTrigger("Switch");
    }

    public void ClearClockPanel()
    {
        this.GetComponent<Animator>().SetTrigger("Clear");
    }
}
