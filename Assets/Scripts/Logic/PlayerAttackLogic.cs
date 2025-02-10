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

    
    

    //コンストラクタ
    public PlayerAttackLogic(
        PlayerAnimLogic playerAnimLogic,
        IAnimationAdapter animationAdapter,
        IObjectData objectData,
        IPlayerStatusAdapter playerStatusAdapter
    ){
        this.playerAnimLogic = playerAnimLogic;
        this.animationAdapter = animationAdapter;
        this.objectData = objectData;
        this.playerStatusAdapter = playerStatusAdapter;                
    }

    public async void Attack(){
        playerAnimLogic.SetAttackAnimation();
        DealDamage(CharacterManager.i.GetObjectByPosition(GetTargetPosition()));
        await Task.Delay(300);
        ActionEventManager.NotifyActionComplete();
    }

    private Vector2Int GetTargetPosition(){
        int selfXpos = Mathf.FloorToInt(objectData.Position.x);
        int selfYpos = Mathf.FloorToInt(objectData.Position.y);
        int faceXpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.x);
        int faceYpos = (int)Mathf.Round(animationAdapter.MoveAnimationDirection.y);
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
        if(playerStatusAdapter.EquipWeapon != null){
            weaponPw = playerStatusAdapter.EquipWeapon.power;
        } else{
            weaponPw = 1;
        }

        IMonsterStatusAdapter monsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
        int damage = damageCalculate.CalculateAttackDamage(playerStatusAdapter.Level, playerStatusAdapter.Muscle, weaponPw, monsterStatusAdapter.Defence);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, objectData.Name);
    }
    
}
