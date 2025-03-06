using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerAttackLogic
{
    private PlayerAnimLogic playerAnimLogic;
    private IAnimationAdapter animationAdapter;
    private IObjectData objectData;
    private IPlayerStatusAdapter playerStatusAdapter;    
    private DamageCalculate damageCalculate;
    private bool isAttacking = false;
    private GameEvent OnPlayerStateComplete;    
    private Player player;
    

    //コンストラクタ
    public PlayerAttackLogic(
        PlayerAnimLogic playerAnimLogic,
        IAnimationAdapter animationAdapter,
        IObjectData objectData,
        IPlayerStatusAdapter playerStatusAdapter,        
        GameEvent OnPlayerStateComplete,
        Player player
    ){
        this.playerAnimLogic = playerAnimLogic;
        this.animationAdapter = animationAdapter;
        this.objectData = objectData;
        this.playerStatusAdapter = playerStatusAdapter;                
        this.OnPlayerStateComplete = OnPlayerStateComplete;        
        this.player = player;
    }

    public void Attack(){
        if(isAttacking) return;
        playerAnimLogic.SetAttackAnimation();
        DealDamage(CharacterManager.i.GetObjectByPosition(GetTargetPosition())); 
        LockInputWhileAttacking();
        OnPlayerStateComplete.Raise();
    }

    private Vector2Int GetTargetPosition(){
        int selfXpos = Mathf.FloorToInt(player.Position.Value.x);
        int selfYpos = Mathf.FloorToInt(player.Position.Value.y);
        int faceXpos = (int)Mathf.Round(player.MoveAnimationDirection.x);
        int faceYpos = (int)Mathf.Round(player.MoveAnimationDirection.y);
        Vector2Int targetVector = new Vector2Int(selfXpos + faceXpos, selfYpos + faceYpos);
        return targetVector;
    }

    private void DealDamage(GameObject targetObject){
        if(targetObject == null) return;
        if(!targetObject.CompareTag("Enemy"))return;
        if(damageCalculate == null){
            damageCalculate = new DamageCalculate();
        }

        int weaponPw;
        if(player.EquipWeapon != null){
            weaponPw = player.EquipWeapon.attackPower;
        } else{
            weaponPw = 1;
        }

        IMonsterStatusAdapter monsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
        int damage = damageCalculate.CalculateAttackDamage(player.playerLevel.Value, player.playerMaxMuscle.Value, weaponPw, monsterStatusAdapter.Defence);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, player.Name.Value);
    }

    async void LockInputWhileAttacking(){
        isAttacking = true;
        await Task.Delay(500);
        isAttacking = false;
    }
    
}
