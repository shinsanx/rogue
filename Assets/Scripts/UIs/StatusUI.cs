using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI maxHPTxt;
    [SerializeField] TextMeshProUGUI currentHPTxt;
    [SerializeField] TextMeshProUGUI floorTxt;
    [SerializeField] TextMeshProUGUI currentLvTxt;
    [SerializeField] TextMeshProUGUI currentGoldTxt;
    [SerializeField] Image healthBarGage;
    
    [SerializeField] IntVariable playerCurrentHealth;
    [SerializeField] IntVariable playerMaxHealth;
    [SerializeField] IntVariable playerLevel;
    [SerializeField] IntVariable playerExperience;    
    [SerializeField] StringVariable playerName;
    [SerializeField] CreateMessageLogic createMessageLogic;
    [SerializeField] MessageEventChannelSO onMessageSend;

    //本当はロジックに移行したい。StatusUILogicとか作って
    public void UpdateHPText() {
        string updateTxt = null;
        maxHPTxt.text = updateTxt.ConvertNumToUpperString(playerMaxHealth.Value);
        currentHPTxt.text = updateTxt.ConvertNumToUpperString(playerCurrentHealth.Value);

        healthBarGage.fillAmount = (float)playerCurrentHealth.Value / (float)playerMaxHealth.Value;
    }

    public void UpdateLvText() {
        string updateTxt = null;
        currentLvTxt.text = updateTxt.ConvertNumToUpperString(playerLevel.Value);
        if (playerLevel.Value == 1) return;

        onMessageSend.RaiseEvent(createMessageLogic.LvUppedMessage(playerName.Value, playerLevel.Value));
    }
    
}
