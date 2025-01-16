using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class EnemyStatusLogic
{
    private IMonsterStatusAdapter monsterStatusAdapter;
    private MonsterStatusSO monsterSO;
    private CreateMessageLogic createMessageLogic;
    public Action OnDestroyed;
    private List<string> messages = new List<string>();

    public EnemyStatusLogic(
        IMonsterStatusAdapter monsterStatusAdapter,
        MonsterStatusSO monsterSO
        ) {
            this.monsterStatusAdapter = monsterStatusAdapter;
            this.monsterSO = monsterSO;
            createMessageLogic = new CreateMessageLogic();
            UpdateEnemyStatus(monsterStatusAdapter, monsterSO);
        }

    private void UpdateEnemyStatus(IMonsterStatusAdapter monsterStatusAdapter, MonsterStatusSO monsterStatusSO){
        monsterStatusAdapter.Name = monsterStatusSO.Name;
        monsterStatusAdapter.HP = monsterStatusSO.HP;
        monsterStatusAdapter.AttackPower = monsterStatusSO.AttackPower;
        monsterStatusAdapter.Defence = monsterStatusSO.Deffence;
        monsterStatusAdapter.Exp = monsterStatusSO.Exp;
        monsterStatusAdapter.MoveSpeed = monsterStatusSO.MoveSpeed;
        monsterStatusAdapter.Attribute = monsterStatusSO.Attribute;
        monsterStatusAdapter.ItemDropRate = monsterStatusSO.ItemDropRate;
        monsterStatusAdapter.AnimatorController = monsterStatusSO.AnimatorController;
        monsterStatusAdapter.Sprite = monsterStatusSO.Sprite;
    }

    public void TakeDamage(int damage, string dealerName){
        monsterStatusAdapter.HP -= damage;

        //TODO: dealerのタグによってメッセージを変える。プレイヤーかその他か
        messages.Clear();
        messages = createMessageLogic.CreateAttackMessage(messages, damage, dealerName, monsterStatusAdapter.Name);

        if(monsterStatusAdapter.HP <= 0){
            messages = createMessageLogic.CreateDefeatedMessage(messages, monsterStatusAdapter.Name, monsterStatusAdapter.Exp);
            MessageBus.Instance.Publish(DungeonConstants.GetExp, monsterStatusAdapter.Exp);
        }
        MessageBus.Instance.Publish("sendMessage", messages);
    }

    public async void Destroy(){
        await Task.Delay(200);
        OnDestroyed();
    }

}
