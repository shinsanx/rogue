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

    [SerializeField] private WeaponSO playerSword;
    [SerializeField] private ShieldSO playerShield;
    [SerializeField] private IntVariable playerCurrentMuscle;
    [SerializeField] private IntVariable playerCurrentLv;
    [SerializeField] private IntVariable playerCurrentExp;

    
    public void OpenStatusMenu() {
        StatusMenuObject.SetActive(true);
        UpdateStatus();
    }

    public void CloseStatusMenu() {
        StatusMenuObject.SetActive(false);
    }

    private void UpdateStatus() {
        if(playerSword != null) {
            SwordPowerText.text = playerSword.attackPower.ToString();
        } else{
            SwordPowerText.text = "---";
        }
        if(playerShield != null) {
            ShieldPowerText.text = playerShield.defensePower.ToString();
        } else{
            ShieldPowerText.text = "---";
        }
        MuscleText.text = playerCurrentMuscle.Value.ToString();
        ExpText.text = playerCurrentExp.Value.ToString();
        int nextExp = DungeonConstants.necessarryExp[playerCurrentLv.Value +1];
        NextExpText.text = (nextExp-playerCurrentExp.Value).ToString();
    }
}
