using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;

public class EnemyAttackLogic {
    private EnemyAnimLogic enemyAnimLogic;
    private IObjectData objectData;
    private IPlayerStatusAdapter playerStatusAdapter;

    private DamageCalculate damageCalculate;
    private Enemy enemy;
    private bool isAttacking = false;


    private int targetDefencePw;

    public EnemyAttackLogic(
        Enemy enemy
    ) {
        this.enemy = enemy;
        this.enemyAnimLogic = enemy.enemyAnimLogic;
        this.objectData = enemy.objectData;
    }

    public void Attack(GameObject go, Vector2Int direction) {
        DealDamage(go, direction);
    }

    public async Task AttackAsync(GameObject go, Vector2Int direction, MonsterStatusSO statusSO) {
        if (isAttacking) return;
        isAttacking = true;        

        var tcs = new TaskCompletionSource<bool>();

        float approach = 0f;
        switch (statusSO.ApproachType) {
            case ApproachType.None: approach = 0f; break;
            case ApproachType.Short: approach = 0.2f; break;
            case ApproachType.Long: approach = 0.5f; break;
            case ApproachType.Custom: approach = statusSO.CustomApproachDistance; break;
        }

        enemyAnimLogic.SetAttackAnimation(direction, approach, OnComplete: () => tcs.TrySetResult(true));

        DealDamage(go, direction);
        await tcs.Task;

        //await Task.Delay(50); //Attackを遅らせるなら追加
        isAttacking = false;
    }



    private void DealDamage(GameObject targetObject, Vector2Int direction) {
        if (targetObject == null) {
            enemyAnimLogic.SetAttackAnimation(direction);
            Debug.Log("攻撃対象がnull");
            return;
        }
        if (damageCalculate == null) {
            damageCalculate = new DamageCalculate();
        }
        //enemyAnimLogic.SetAttackAnimation(direction);

        if (targetObject.CompareTag("Player")) {
            if (playerStatusAdapter == null) playerStatusAdapter = targetObject.GetComponent<IPlayerStatusAdapter>();
            if (playerStatusAdapter.EquipShield != null) {
                targetDefencePw = playerStatusAdapter.EquipShield.defensePower;
            } else {
                targetDefencePw = 1;
            }
        }

        if (targetObject.CompareTag("Enemy")) {
            IMonsterStatusAdapter targetMonsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
            targetDefencePw = targetMonsterStatusAdapter.Defence;
        }

        int damage = damageCalculate.CalculateEnemyAttackDamage(enemy.AttackPower, targetDefencePw);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, objectData.Name.Value);
    }
}
