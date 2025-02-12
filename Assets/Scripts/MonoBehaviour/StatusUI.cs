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

    private Player player;

    private void OnDestroy() {
        if(player != null){
            player.OnHealthChanged -= UpdateHPText;
            player.OnLvChanged -= UpdateLvText;
        }
    }

    //本当はロジックに移行したい。StatusUILogicとか作って
    public void UpdateHPText(int currentHp, int maxHp){
        string updateTxt = null;
        maxHPTxt.text = updateTxt.ConvertNumToUpperString(maxHp);
        currentHPTxt.text = updateTxt.ConvertNumToUpperString(currentHp);

        healthBarGage.fillAmount = (float)currentHp / (float)maxHp;
    }

    public void UpdateLvText(int level){        
        string updateTxt = null;
        currentLvTxt.text = updateTxt.ConvertNumToUpperString(level);
    }

    public void Initialize(){
        player = FindObjectOfType<Player>();
        if(player != null){            
            player.OnHealthChanged += UpdateHPText;
            player.OnLvChanged += UpdateLvText;
        }
    }
}
