using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusDataLogic
{    
    private List<string> messages = new List<string>();
    private Player player;
    private CreateMessageLogic createMessageLogic;
    private MessageEventChannelSO onMessageSend;
    public PlayerStatusDataLogic(Player player, CreateMessageLogic createMessageLogic, MessageEventChannelSO onMessageSend){        
        this.player = player;
        this.createMessageLogic = createMessageLogic;
        this.onMessageSend = onMessageSend;
    }    

    // public async void GetExp(object exp){
    //     await System.Threading.Tasks.Task.Delay(1500); //最悪！メッセージシステム自体直す
    //     player.ChangePlayerExperience((int)exp);
    // }

    public void LevelUp(){        
        int randUpHP = Random.Range(3,7);
        player.ChangePlayerMaxHealth(player.playerMaxHealth.Value + randUpHP);        
    }

    

    public void TakeDamage(int damage, string dealerName){
        player.ChangePlayerCurrentHealth(player.playerCurrentHealth.Value - damage);        

        //Todo: dealerのタグによってメッセージを変える。Enemyかその他か
        messages.Clear();
        messages = createMessageLogic.CreateTakeDamageMessage(messages, damage, dealerName);

        onMessageSend.RaiseEvent(messages);
    }
}
