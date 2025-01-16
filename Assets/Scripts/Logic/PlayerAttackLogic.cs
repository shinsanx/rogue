using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerAttackLogic
{
    private PlayerAnimLogic playerAnimLogic;
    private IAnimationAdapter animationAdapter;
    private IPositionAdapter positionAdapter;
    private IPlayerStatusAdapter playerStatusAdapter;
    private TileLogic tileLogic;
    private DamageCalculate damageCalculate;

    private StateMachine stateMachine;
    private State enemyState;

    //コンストラクタ
    public PlayerAttackLogic(
        PlayerAnimLogic playerAnimLogic,
        IAnimationAdapter animationAdapter,
        IPositionAdapter positionAdapter,
        IPlayerStatusAdapter playerStatusAdapter
    ){
        this.playerAnimLogic = playerAnimLogic;
        tileLogic = new TileLogic();
        this.animationAdapter = animationAdapter;
        this.positionAdapter = positionAdapter;
        this.playerStatusAdapter = playerStatusAdapter;
        stateMachine = GameAssets.i.stateMachine;
        enemyState = GameAssets.i.enemyState;
    }

    public void Attack(){
        playerAnimLogic.SetAttackAnimation();
        DealDamage(GetTarget());
        EndState();
    }

    private GameObject GetTarget(){
        int selfXpos = Mathf.FloorToInt(positionAdapter.Position.x);
        int selfYpos = Mathf.FloorToInt(positionAdapter.Position.y);
        int targetXpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.x);
        int targetYpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.y);
        Vector2Int targetVector = new Vector2Int(selfXpos + targetXpos, selfYpos + targetYpos);

        GameObject targetObject = tileLogic.GetObjectOnTile(targetVector);
        return targetObject;
    }

    private void DealDamage(GameObject targetObject){
        if(targetObject == null) return;
        if(!targetObject.CompareTag("Enemy"))return;
        if(damageCalculate == null){
            damageCalculate = new DamageCalculate();
        }

        int weaponPw;
        if(playerStatusAdapter.EquipWeapon != null){
            weaponPw = playerStatusAdapter.EquipWeapon.power;
        } else{
            weaponPw = 1;
        }

        IMonsterStatusAdapter monsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
        int damage = damageCalculate.CalculateAttackDamage(playerStatusAdapter.Level, playerStatusAdapter.Muscle, weaponPw, monsterStatusAdapter.Defence);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, playerStatusAdapter.Name);
    }

    private void EndState(){
        stateMachine.SetState(enemyState);
    }
}
