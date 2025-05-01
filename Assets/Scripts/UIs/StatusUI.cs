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
    [SerializeField] RectTransform healthBarFrame;

    [SerializeField] IntVariable playerCurrentHealth;
    [SerializeField] IntVariable playerMaxHealth;
    [SerializeField] IntVariable playerLevel;
    [SerializeField] IntVariable playerExperience;
    [SerializeField] StringVariable playerName;
    [SerializeField] CreateMessageLogic createMessageLogic;
    [SerializeField] MessageEventChannelSO onMessageSend;

    const float MIN_WIDTH = 80f;
    const float MAX_WIDTH = 350f;
    const int MIN_HP = 20;
    const int MAX_HP = 150;

    void Start() {
        // 左固定にするためのアンカー設定
        healthBarFrame.anchorMin = new Vector2(0, 0.5f); // 左中央
        healthBarFrame.anchorMax = new Vector2(0, 0.5f); // 左中央
        healthBarFrame.pivot = new Vector2(0, 0.5f); // 回転軸も左

        // // 中身も
        var gageRect = healthBarGage.GetComponent<RectTransform>();
        gageRect.anchorMin = new Vector2(0, 0.5f); // 左中央
        gageRect.anchorMax = new Vector2(0, 0.5f); // 左中央
        gageRect.pivot = new Vector2(0, 0.5f); // 回転軸も左

        //位置変更
        healthBarFrame.anchoredPosition = new Vector2(645,-40);
        gageRect.anchoredPosition = new Vector2(645,-40);

    }

    public void UpdateHPText() {
        string updateTxt = null;
        maxHPTxt.text = updateTxt.ConvertNumToUpperString(playerMaxHealth.Value);
        currentHPTxt.text = updateTxt.ConvertNumToUpperString(playerCurrentHealth.Value);

        healthBarGage.fillAmount = (float)playerCurrentHealth.Value / (float)playerMaxHealth.Value;

        // 枠と中身の幅をHPに合わせて線型補完
        float t = Mathf.InverseLerp(MIN_HP, MAX_HP, playerCurrentHealth.Value);
        float width = Mathf.Lerp(MIN_WIDTH, MAX_WIDTH, t);

        //左固定で幅だけ変える
        healthBarFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        healthBarGage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public void UpdateLvText() {
        string updateTxt = null;
        currentLvTxt.text = updateTxt.ConvertNumToUpperString(playerLevel.Value);
        if (playerLevel.Value > 1) { }

        onMessageSend.RaiseEvent(createMessageLogic.LvUppedMessage(playerName.Value, playerLevel.Value));
    }
}
    
