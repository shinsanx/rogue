using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusDataLogic
{
    private IPlayerStatusAdapter playerStatusAdapter;
    private IObjectData objectData;
    private IAnimationAdapter animationAdapter;
    private List<string> messages = new List<string>();
    CreateMessageLogic createMessageLogic;

    public PlayerStatusDataLogic(IPlayerStatusAdapter playerStatusAdapter, IAnimationAdapter animationAdapter, IObjectData objectData){
        this.playerStatusAdapter = playerStatusAdapter;
        this.animationAdapter = animationAdapter;
        this.objectData = objectData;
        createMessageLogic = new CreateMessageLogic();
    }    

    public async void GetExp(object exp){
        await System.Threading.Tasks.Task.Delay(1500); //最悪！メッセージシステム自体直す
        playerStatusAdapter.Experience += (int)exp;
    }

    public void LevelUp(){
        if(playerStatusAdapter.Level == 1){
            return;
        }
        int randUpHP = Random.Range(3,7);
        playerStatusAdapter.MaxHealth += randUpHP;
        playerStatusAdapter.health += randUpHP;
    }

    public void SetStatusDefault(IPlayerStatusAdapter playerStatusAdapter){        
        playerStatusAdapter.Level = 1;
        playerStatusAdapter.MaxHealth = 15;
        playerStatusAdapter.health = 15;
        playerStatusAdapter.MaxSatiety = 100;
        playerStatusAdapter.Satiety = 100;
        playerStatusAdapter.MaxMuscle = 8;
        playerStatusAdapter.Muscle = 8;
        playerStatusAdapter.BasicAttackPower = 0;
        playerStatusAdapter.DefencePower = 0;
        playerStatusAdapter.Experience = 0;
        playerStatusAdapter.EquipWeapon = null;
        playerStatusAdapter.EquipShield = null;
    }

    public void SetObjectDataDefault(IObjectData objectData){
        objectData.Name = "トルネコ";   
        objectData.Type = "Player";

    }

    public void TakeDamage(int damage, string dealerName){
        playerStatusAdapter.health -= damage;
        animationAdapter.TakeDamageAnimation = true;

        //Todo: dealerのタグによってメッセージを変える。Enemyかその他か
        messages.Clear();
        messages = createMessageLogic.CreateTakeDamageMessage(messages, damage, dealerName);

        MessageBus.Instance.Publish("sendMessage", messages);
    }
}
