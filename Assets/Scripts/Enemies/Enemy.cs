using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IDamageable, IMonsterStatusAdapter, IAnimationAdapter, IObjectData, IEnemyAIState, IEffectReceiver {
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] MonsterStatusSO monsterSO;

    private EnemyStatusLogic enemyStatusLogic;
    private EnemyAnimLogic enemyAnimLogic;
    private EnemyAttackLogic enemyAttackLogic;    
    private EnemyMoveLogic enemyMoveLogic;

    Vector2 moveOffset = new Vector2(.5f, .5f);
    private Vector2Int _enemyPosition;
    private int _hp;
    public int MaxHealth { get; set; }

    // ========================================================
    // =================== IPositionAdapter ===================
    // ========================================================
    public Vector2IntVariable Position {
        get { return Position; }
        set {            
             transform.DOMove(Position.Value.ToVector2() + moveOffset, (0.3f)).SetEase(Ease.Linear);
             Position.SetValue(value);
             OnObjectUpdated.Invoke(this);
        }
    }

    // ========================================================
    // ===================== IObjectData =====================
    // ========================================================
    //public int Id { get; set; }
    //public string Name { get; set; }
    //string IObjectData.Type { get; set; }
    //int IObjectData.RoomNum { get; set; }

    public IntVariable Id{get;set;}
    public StringVariable Name{get;set;}
    public StringVariable Type{get;set;}
    public IntVariable RoomNum{get;set;}    
    
    // オブジェクトが更新された時に呼び出されるイベント
    // subscribeしているのは
    public event System.Action<IObjectData> OnObjectUpdated;

    // ========================================================
    // ================== IAnimationAdapter ==================
    // ========================================================
    private Vector2 enemyFaceDirection;
    public Vector2 MoveAnimationDirection {
        get { return enemyFaceDirection; }
        set {
            enemyFaceDirection = new Vector2(value.x, value.y);
            SetAnimatorDirection(value);
        }
    }        
    private void SetAnimatorDirection(Vector2 direction) {
        animator.SetFloat("x", direction.x);
        animator.SetFloat("y", direction.y);
    }
    //攻撃時に再生されるアニメーション    
    public bool AttackAnimation {
        set { TriggerAnimator("AtkTrigger"); }
    }
    //ダメージを受けた時に再生されるアニメーション
    public bool TakeDamageAnimation {
        set { TriggerAnimator("TakeDamageTrigger"); }
    }
    private void TriggerAnimator(string triggerName) {
        animator.SetTrigger(triggerName);
    }    
    public bool EatAnimation {
        get;set;        
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
        enemyAnimLogic = new EnemyAnimLogic(this, sr);
        enemyMoveLogic = new EnemyMoveLogic(this, enemyAnimLogic);
        enemyAttackLogic = new EnemyAttackLogic(enemyAnimLogic, this, this, this);        

        enemyStatusLogic.InitializeEnemyStatus(this, monsterSO, this);
        Id.SetValue(CharacterManager.GetUniqueID()); // Assign a unique ID
        CharacterManager.i.AddCharacter(this);

        //MoveAnimationDirection = new Vector2(0,-1); //初期の方向　仮で一旦下を向くように
    }


    private void OnEnemyDestroyed() {
        CharacterManager.i.RemoveCharacter(this);
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, string dealerName) {
        enemyStatusLogic.TakeDamage(damage, dealerName);
    }

    private void OnDisable() {
        enemyAnimLogic.KillTween();
    }

    private void OnEnable() {
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
