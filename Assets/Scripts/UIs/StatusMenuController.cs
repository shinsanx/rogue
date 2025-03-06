using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SwordPowerText;
    [SerializeField] private TextMeshProUGUI ShieldPowerText;
    [SerializeField] private TextMeshProUGUI MuscleText;
    [SerializeField] private TextMeshProUGUI ExpText;
    [SerializeField] private TextMeshProUGUI NextExpText;
    
    [SerializeField] private GameObject StatusMenuObject;
    
    public void OpenStatusMenu() {
        StatusMenuObject.SetActive(true);
        UpdateStatus();
    }

    public void CloseStatusMenu() {
        StatusMenuObject.SetActive(false);
    }

    private void UpdateStatus() {
        // SwordPowerText.text = Player.i.SwordPower.ToString();
        // ShieldPowerText.text = Player.i.ShieldPower.ToString();
        // MuscleText.text = Player.i.Muscle.ToString();
        // ExpText.text = Player.i.Exp.ToString();
        // NextExpText.text = Player.i.NextExp.ToString();
    }
}
