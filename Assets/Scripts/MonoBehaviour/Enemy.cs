using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IDamageable, IMonsterStatusAdapter, IAnimationAdapter, IObjectData {
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] MonsterStatusSO monsterSO;    
    
    private EnemyStatusLogic enemyStatusLogic;
    private EnemyAnimLogic enemyAnimLogic;
    private EnemyAttackLogic enemyAttackLogic;
    private EnemyAILogic enemyAILogic;
    private EnemyMoveLogic enemyMoveLogic;

    Vector2 moveOffset = new Vector2(.5f, .5f);
    private Vector2Int _enemyPosition; //startでtransform.position入れてるけど危険
    private int _hp;




    //=== IPositionAdapter ===
    public Vector2Int Position {
        get { return _enemyPosition; }
        set {
            transform.DOMove(value.ToVector2() + moveOffset, (0.3f)).SetEase(Ease.Linear);
            _enemyPosition = value;
            OnObjectUpdated.Invoke(this);            
        }
    }


    // === IAnimationAdapter ===
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

    string IObjectData.Name { get; set; }

    int IMonsterStatusAdapter.HP {
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

    public bool Action {
        get { return true; }
        set { enemyAILogic.AIStart(); }
    }

    // IObjectData
    public int Id { get; set; }
    public string Name { get; set; }
    string IObjectData.Type { get; set; }
    int IObjectData.RoomNum { get; set; }

    public event System.Action<IObjectData> OnObjectUpdated;

    private void Awake() {
        InitializeEnemy();
        Position = transform.position.ToVector2Int();
    }

    private void InitializeEnemy() {
        _enemyPosition = transform.position.ToVector2Int();
        enemyAnimLogic = new EnemyAnimLogic(this, sr);
        enemyStatusLogic = new EnemyStatusLogic(this, monsterSO);
        enemyMoveLogic = new EnemyMoveLogic(this, enemyAnimLogic);
        enemyStatusLogic.OnDestroyed += OnEnemyDestroyed;
        enemyAttackLogic = new EnemyAttackLogic(enemyAnimLogic, this, this, this);
        enemyAILogic = new EnemyAILogic(this, enemyAttackLogic, enemyMoveLogic, new AStarPathfinding());
        enemyStatusLogic.InitializeEnemyStatus(this, monsterSO, this);
        CharacterManager.i.AddCharacter(this);
        //MoveAnimationDirection = new Vector2(0,-1); //初期の方向　仮で一旦下を向くように

    }


    private void OnEnemyDestroyed(){
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
        Id = CharacterManager.GetUniqueID(); // Assign a unique ID
    }
}
