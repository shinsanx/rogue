using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class EnemyStatusLogic
{
    private Enemy enemy;
    private IMonsterStatusAdapter monsterStatusAdapter;
    private MonsterStatusSO monsterSO;
    private CreateMessageLogic createMessageLogic;
    public Action OnDestroyed;
    private List<string> messages = new List<string>();
    private MessageEventChannelSO onMessageSend;
    private IntEventChannelSO addExp;


    public EnemyStatusLogic(
        IMonsterStatusAdapter monsterStatusAdapter,
        MonsterStatusSO monsterSO,
        MessageEventChannelSO onMessageSend,
        IntEventChannelSO addExp
        ) {
            this.monsterStatusAdapter = monsterStatusAdapter;
            this.monsterSO = monsterSO;
            this.onMessageSend = onMessageSend;
            this.addExp = addExp;
        }

    public void InitializeEnemyStatus(IMonsterStatusAdapter monsterStatusAdapter, MonsterStatusSO monsterStatusSO, Enemy enemy, CreateMessageLogic createMessageLogic){        
        this.createMessageLogic = createMessageLogic;
        monsterStatusAdapter.HP = monsterStatusSO.HP;
        monsterStatusAdapter.MaxHealth = monsterStatusSO.HP;
        monsterStatusAdapter.AttackPower = monsterStatusSO.AttackPower;
        monsterStatusAdapter.Defence = monsterStatusSO.Deffence;
        monsterStatusAdapter.Exp = monsterStatusSO.Exp;
        monsterStatusAdapter.MoveSpeed = monsterStatusSO.MoveSpeed;
        monsterStatusAdapter.Attribute = monsterStatusSO.Attribute;
        monsterStatusAdapter.ItemDropRate = monsterStatusSO.ItemDropRate;
        monsterStatusAdapter.AnimatorController = monsterStatusSO.AnimatorController;
        
        monsterStatusAdapter.Sprite = monsterStatusSO.Sprite;

        enemy.objectData.Name.SetValue(monsterStatusSO.Name);
        enemy.objectData.Type.SetValue("Enemy");        
    }

    
    
    

    public void TakeDamage(int damage, string dealerName){
        monsterStatusAdapter.HP -= damage;

        //TODO: dealerのタグによってメッセージを変える。プレイヤーかその他か
        messages.Clear();
        messages = createMessageLogic.CreateAttackMessage(messages, damage, dealerName, monsterSO.Name);

        if(monsterStatusAdapter.HP <= 0){
            messages = createMessageLogic.CreateDefeatedMessage(messages, monsterSO.Name, monsterStatusAdapter.Exp);
            addExp.RaiseEvent(monsterStatusAdapter.Exp);
        }
        onMessageSend.RaiseEvent(messages);
    }

    public async void Destroy(){
        await Task.Delay(200);
        OnDestroyed();
    }

}
