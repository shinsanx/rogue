using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAttackLogic
{
    private EnemyAnimLogic enemyAnimLogic;
    private IAnimationAdapter animationAdapter;
    private IObjectData objectData;
    private IPlayerStatusAdapter playerStatusAdapter;

    private DamageCalculate damageCalculate;
    private IMonsterStatusAdapter monsterStatusAdapter;    

    private StateMachine stateMachine;
    private State enemyState;
    private int targetDefencePw;

    public EnemyAttackLogic(
        EnemyAnimLogic enemyAnimLogic,
        IAnimationAdapter animationAdapter,
        IObjectData objectData,
        IMonsterStatusAdapter monsterStatusAdapter
        ) {
            this.enemyAnimLogic = enemyAnimLogic;
            this.animationAdapter = animationAdapter;
            this.objectData = objectData;
            this.monsterStatusAdapter = monsterStatusAdapter;
            stateMachine = GameAssets.i.stateMachine;
            enemyState = GameAssets.i.enemyState;            
    }

    public void Attack(GameObject go, Vector2Int direction){
        Debug.Log("Attack by EnemyAttackLogic");
        DealDamage(go, direction);        
    }

    private GameObject GetTarget(){
        int selfXpos = Mathf.FloorToInt(objectData.Position.x);
        int selfYpos = Mathf.FloorToInt(objectData.Position.y);
        int targetXpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.x);
        int targetYpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.y);

        Vector2Int targetVector = new Vector2Int(selfXpos + targetXpos, selfYpos + targetYpos);
        GameObject targetObject = CharacterManager.i.GetObjectByPosition(targetVector);

        return targetObject;        
    }

    private void DealDamage(GameObject targetObject, Vector2Int direction){
        if(targetObject == null)return;
        if(damageCalculate == null){
            damageCalculate = new DamageCalculate();
        }
        enemyAnimLogic.SetAttackAnimation(direction);

        if(targetObject.CompareTag("Player")){
            if(playerStatusAdapter == null)playerStatusAdapter = targetObject.GetComponent<IPlayerStatusAdapter>();
            if(playerStatusAdapter.EquipShield != null){
                targetDefencePw = playerStatusAdapter.EquipShield.power;
            } else {
                targetDefencePw = 1;
            }
        }

        if(targetObject.CompareTag("Enemy")){
            IMonsterStatusAdapter targetMonsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
            targetDefencePw = targetMonsterStatusAdapter.Defence;
        }

        int damage = damageCalculate.CalculateEnemyAttackDamage(monsterStatusAdapter.AttackPower, targetDefencePw);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, objectData.Name);
    }
}
