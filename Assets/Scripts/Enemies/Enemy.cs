using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IDamageable, IMonsterStatusAdapter, IEnemyAIState, IEffectReceiver {
    public Animator animator;
    public SpriteRenderer sr;
    public MonsterStatusSO monsterSO;    

    public EnemyStatusLogic enemyStatusLogic;
    public EnemyAnimLogic enemyAnimLogic;
    public EnemyAttackLogic enemyAttackLogic;    
    public EnemyMoveLogic enemyMoveLogic;
    public ObjectData objectData;
    public AnimationAdapter animationAdapter;
    Vector2 moveOffset = new Vector2(.5f, .5f);
    private int _hp;
    public int MaxHealth { get; set; }
    

    public void MovePosition() {        
        transform.DOMove(objectData.Position.Value.ToVector2() + moveOffset, 0.3f)
            .SetEase(Ease.Linear);        
    }  


    // ========================================================
    // ================= IMonsterStatusAdapter ================
    // ========================================================
    public int HP {
        get { return _hp; }
        set {
            if (_hp <= value) {
                //TODO: 回復アニメーションを再生する＆ダメージアニメーションを再生しない
            }
            _hp = value;
            
            enemyAnimLogic.SetDamageAnimation();
            if (_hp <= 0) {
                enemyAnimLogic.SetDamageAnimation();
                enemyStatusLogic.Destroy();
                //TODO: 敵が死亡した場合の処理
            }
        }
    }
    public int AttackPower { get; set; }
    public int Defence { get; set; }
    public int Exp { get; set; }
    public string MoveSpeed { get; set; }
    public string Attribute { get; set; }
    public float ItemDropRate { get; set; }

    public Sprite Sprite {
        get { return null; }
        set { sr.sprite = value; }
    }
    public RuntimeAnimatorController AnimatorController {
        get { return animator.runtimeAnimatorController; }
        set { animator.runtimeAnimatorController = value; }
    }
    

    // ========================================================
    // ==================== IEnemyAIState =================
    // ========================================================
    public bool IsInRoomAtStart { get; set; }
    public bool IsInRoomAtEnd { get; set; }
    public bool IsAdjacentToPlayerAtStart { get; set; }
    public bool IsAdjacentToPlayerAtEnd { get; set; }
    public bool CanSeePlayer { get; set; }
    public Vector2Int FacingDirection { get; set; }
    public Vector2Int EnterJointPosition { get; set; }
    public Vector2Int LastKnownPlayerPosition { get; set; }
    public Vector2Int TargetPosition { get; set; }
    public Vector2Int StartPosition { get; set; }
    public Vector2Int EndPosition { get; set; }
    public List<Vector2Int> MonsterView { get; set; }
    public List<Vector2Int> RouteCache { get; set; }    


    // ========================================================
    // ===================== Methods =====================
    // ========================================================
    public void ExecuteAction(EnemyAction action)
    {        
        switch(action.Type)
        {
            case ActionType.Move:
                enemyMoveLogic.Move(action.TargetPosition, action.Direction);
                break;
            case ActionType.Attack:                
                enemyAttackLogic.Attack(action.Target, action.Direction);
                break;
        }
    }

    public void InitializeEnemy() {
        enemyStatusLogic = new EnemyStatusLogic(this, monsterSO);
        enemyStatusLogic.OnDestroyed += OnEnemyDestroyed;
        enemyAnimLogic = new EnemyAnimLogic(animationAdapter, sr);
        enemyMoveLogic = new EnemyMoveLogic(this);
        enemyAttackLogic = new EnemyAttackLogic(this);        

        enemyStatusLogic.InitializeEnemyStatus(this, monsterSO, this);
        objectData.SetId(CharacterManager.GetUniqueID()); // Assign a unique ID                

        //MoveAnimationDirection = new Vector2(0,-1); //初期の方向　仮で一旦下を向くように
    }


    private void OnEnemyDestroyed() {        
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, string dealerName) {
        enemyStatusLogic.TakeDamage(damage, dealerName);
    }

    private void OnDisable() {
        enemyAnimLogic.KillTween();
    }

    

    // ========================================================
    // ==================== IEffectReceiver ===================
    // ========================================================
    public void ApplyEffect(EffectSO effect) {
        effect.ApplyEffect(this);
    }

    public void Heal(int amount) {
        HP += amount;
    }
}
