using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Threading.Tasks;

[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IDamageable, IMonsterStatusAdapter, IEnemyAIState, IEffectReceiver {
    public Animator animator;
    public SpriteRenderer sr;
    public MonsterStatusSO monsterSO;
    [SerializeField] private CreateMessageLogic createMessageLogic;
    [SerializeField] private MessageEventChannelSO onMessageSend;
    [SerializeField] private IntEventChannelSO addExp;
    public EnemyStatusLogic enemyStatusLogic;
    public EnemyAnimLogic enemyAnimLogic;
    public EnemyAttackLogic enemyAttackLogic;
    public EnemyMoveLogic enemyMoveLogic;
    public ObjectData objectData;
    public AnimationAdapter animationAdapter;
    Vector2 moveOffset = new Vector2(.5f, .5f);
    private int _hp;
    public int MaxHealth { get; set; }
    [SerializeField] private FloatVariable moveSpeed;


    public void MovePosition() {
        transform.DOMove(objectData.Position.Value.ToVector2() + moveOffset, moveSpeed.Value)
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
    public const int Threshold = 100; //行動の閾値
    public int actionRate { get; set; } = 50;
    public int TimeGage = 0;


    // ========================================================
    // ===================== Methods =====================
    // ========================================================
    // public void ExecuteAction(EnemyAction action) {
    //     if (action == null) {
    //         return;
    //     }
    //     switch (action.Type) {
    //         case ActionType.Move:
    //             enemyMoveLogic.Move(action.TargetPosition, action.Direction);
    //             break;
    //         case ActionType.Attack:
    //             enemyAttackLogic.Attack(action.Target, action.Direction);
    //             break;
    //         case ActionType.Sleep:
    //             Debug.Log("ねむっています");
    //             break;
    //     }
    //     TimeGage -= Threshold; //行動したらゲージ消費 
    // }

    public void InitializeEnemy() {
        CreateSOInstance();
        enemyStatusLogic = new EnemyStatusLogic(this, monsterSO, onMessageSend, addExp);
        enemyStatusLogic.OnDestroyed += OnEnemyDestroyed;
        enemyAnimLogic = new EnemyAnimLogic(animationAdapter, sr);
        enemyMoveLogic = new EnemyMoveLogic(this);
        enemyAttackLogic = new EnemyAttackLogic(this);

        enemyStatusLogic.InitializeEnemyStatus(this, monsterSO, this, createMessageLogic);
        //objectData.SetId(CharacterManager.GetUniqueID()); // Assign a unique ID                

        //MoveAnimationDirection = new Vector2(0,-1); //初期の方向　仮で一旦下を向くように
    }


    private void OnEnemyDestroyed() {
        enemyAnimLogic.KillTween();
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, string dealerName) {
        enemyStatusLogic.TakeDamage(damage, dealerName);
    }

    private void OnDisable() {
        enemyAnimLogic.KillTween();
    }

    private void CreateSOInstance() {
        objectData.CreateSOInstance();
        animationAdapter.CreateSOInstance();

        isConfusion = ScriptableObject.CreateInstance<BoolVariable>();
        isSleeping = ScriptableObject.CreateInstance<BoolVariable>();
    }


    //行動可能かどうかを返す
    public bool CanAct() {
        return TimeGage >= Threshold;
    }

    //ゲージを加算する
    public void Tick() {
        TimeGage += actionRate;
        TickStatusEffects();
    }

    public async Task ExecuteActionAsync(EnemyAction action) {
        switch (action.Type) {
            case ActionType.Attack:
                Debug.Log("start");
                await enemyAttackLogic.AttackAsync(action.Target, action.Direction, monsterSO);
                Debug.Log("end");
                break;

            case ActionType.Move:
                enemyMoveLogic.Move(action.TargetPosition, action.Direction);
                break;

            case ActionType.Sleep:
                await Task.Delay(200);          // ただ待つだけでも OK
                break;
        }
        TimeGage = 0; //行動したらゲージ消費 

        // ここに来た時点で完了
    }



    // ========================================================
    // ==================== IEffectReceiver ===================
    // ========================================================    

    public void Heal(int amount, int maxUpAmount) {
        HP += amount;
    }

    public void Equip(BaseItemSO item) {
        //TODO: 敵がアイテムを装備する?
    }

    public void MuscleHeal() {
        //TODO: 敵がちからを回復する?
    }

    // ================================================
    // ============== IStatusEffectTarget =============
    // ================================================
    public BoolVariable isConfusion { get; set; }
    public BoolVariable isSleeping { get; set; }

    private List<StatusEffectInstance> activeEffects = new();

    public List<StatusEffectInstance> GetActiveStatusEffects() {
        return activeEffects;
    }

    public void AddStatusEffect(BaseStatusEffect effect) {
        var instance = new StatusEffectInstance(effect, this);
        activeEffects.Add(instance);
    }

    public void RemoveStatusEffect(BaseStatusEffect effect) {
        var instance = activeEffects.Find(e => e.Effect == effect);
        if (instance != null) {
            instance.EndEffect();
            activeEffects.Remove(instance);
        }
    }

    public void TickStatusEffects() {
        foreach (var instance in activeEffects.ToList()) {
            instance.Tick();
            if (instance.IsExpired) {
                instance.EndEffect();
                activeEffects.Remove(instance);
            }
        }
    }


}
