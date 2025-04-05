using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAttackLogic
{
    private EnemyAnimLogic enemyAnimLogic;    
    private IObjectData objectData;
    private IPlayerStatusAdapter playerStatusAdapter;

    private DamageCalculate damageCalculate;
    private Enemy enemy;
    

    private StateMachine stateMachine;
    private State enemyState;
    private int targetDefencePw;

    public EnemyAttackLogic(
        Enemy enemy
    ) {
        this.enemy = enemy;
        this.enemyAnimLogic = enemy.enemyAnimLogic;
        this.objectData = enemy.objectData;
        stateMachine = GameAssets.i.stateMachine;
        enemyState = GameAssets.i.enemyState;            
    }

    public void Attack(GameObject go, Vector2Int direction){
        DealDamage(go, direction);        
    }

    

    private void DealDamage(GameObject targetObject, Vector2Int direction){        
        if(targetObject == null){
            enemyAnimLogic.SetAttackAnimation(direction);
            return;
        }
        if(damageCalculate == null){
            damageCalculate = new DamageCalculate();
        }
        enemyAnimLogic.SetAttackAnimation(direction);

        if(targetObject.CompareTag("Player")){
            if(playerStatusAdapter == null)playerStatusAdapter = targetObject.GetComponent<IPlayerStatusAdapter>();
            if(playerStatusAdapter.EquipShield != null){
                targetDefencePw = playerStatusAdapter.EquipShield.defensePower;
            } else {
                targetDefencePw = 1;
            }
        }

        if(targetObject.CompareTag("Enemy")){
            IMonsterStatusAdapter targetMonsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
            targetDefencePw = targetMonsterStatusAdapter.Defence;
        }

        int damage = damageCalculate.CalculateEnemyAttackDamage(enemy.AttackPower, targetDefencePw);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, objectData.Name.Value);
    }
}
