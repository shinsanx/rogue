using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI maxHPTxt;
    [SerializeField] TextMeshProUGUI currentHPTxt;
    [SerializeField] TextMeshProUGUI floorTxt;
    [SerializeField] TextMeshProUGUI currentLvTxt;
    [SerializeField] TextMeshProUGUI currentGoldTxt;
    [SerializeField] Image healthBarGage;

    private void Start(){
        MessageBus.Instance.Subscribe(DungeonConstants.UpdateHPText, UpdateHPText);
        MessageBus.Instance.Subscribe(DungeonConstants.UpdateLvText, UpdateLvText);
    }

    //本当はロジックに移行したい。StatusUILogicとか作って
    public void UpdateHPText(object data){
        IPlayerStatusAdapter playerStatusAdapter = (IPlayerStatusAdapter)data;
        string updateTxt = null;
        maxHPTxt.text = updateTxt.ConvertNumToUpperString(playerStatusAdapter.MaxHealth);
        currentHPTxt.text = updateTxt.ConvertNumToUpperString(playerStatusAdapter.health);

        healthBarGage.fillAmount = (float)playerStatusAdapter.health / (float)playerStatusAdapter.MaxHealth;
    }

    public void UpdateLvText(object data){
        IPlayerStatusAdapter playerStatusAdapter = (IPlayerStatusAdapter)data;
        string updateTxt = null;
        currentLvTxt.text = updateTxt.ConvertNumToUpperString(playerStatusAdapter.Level);
    }
}
