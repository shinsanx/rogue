using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour, IAnimationAdapter, IDamageable, IPlayerStatusAdapter, IObjectData, IEffectReceiver {
    // === Serialized Fields ===
    [SerializeField] private RandomDungeonWithBluePrint.RandomMapGenerator randomMapGenerator;
    [SerializeField] private DungeonEventManager dungeonEventManager;
    [SerializeField] private Animator animator;
    [SerializeField] private UserInput userInput;


    // === Private Fields ===
    private PlayerMoveLogic playerMoveLogic;
    private PlayerAttackLogic playerAttackLogic;
    private PlayerAnimLogic playerAnimLogic;
    public PlayerInventory playerInventory;
    private PlayerStatusDataLogic playerStatusDataLogic;
    private CreateMessageLogic createMessageLogic;

    private Vector2 playerFaceDirection;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] bool resetStatus = true;

    private int _currentHp;
    private int _maxHp;
    private int _currentLv;
    private int _totalExp;

    private bool isMoving = false;
    public bool IsMoving() => isMoving;


    // ================================================
    // ==================== Events ====================
    // ================================================            

    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerLevelChanged;

    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerHPChanged;

    // AttackLogic, MoveLogic, Inventoryで実装。
    [SerializeField] GameEvent OnPlayerStateComplete;

    // CharacterManagerのAddCharacterで実装。
    public event Action<IObjectData> OnObjectUpdated; // オブジェクトの更新を渡すイベント

    // ================================================
    // ================== ObjectData ==================
    // ================================================


    [SerializeField] private IntVariable _id;
    [SerializeField] private StringVariable _name;
    [SerializeField] private StringVariable _type;
    [SerializeField] private IntVariable _roomNum;
    [SerializeField] private Vector2IntVariable _position;

    // オブジェクトデータ
    public IntVariable Id { get => _id; set => _id.SetValue(value); }
    public StringVariable Name { get => _name; set => _name.SetValue(value); }
    public StringVariable Type { get => _type; set => _type.SetValue(value); }
    public IntVariable RoomNum { get => _roomNum; set => _roomNum.SetValue(value); }    

    public Vector2IntVariable Position {
        get => _position;
        set {
            _position.SetValue(value);
            // isMoving = false; //本当はtrueにする。デバッグのためにfalseにしている。
            // Debug.Log("Playerの位置を" + _position.Value + "に変更します");
            // transform.DOMove(_position.Value.ToVector2() + moveOffset, 0.3f)
            //     .SetEase(Ease.Linear)
            //     .OnComplete(() => {
            //         isMoving = false;
            //         OnObjectUpdated?.Invoke(this);
            //     });
        }
    }

    public void SetPosition(Vector2Int position) {
        Position.SetValue(position);
        transform.DOMove(position.ToVector2() + moveOffset, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isMoving = false;
                OnObjectUpdated?.Invoke(this);
            });
    }

    // ================================================
    // ============= IAnimationAdapter ===============
    // ================================================
    public bool AttackAnimation {
        set => animator.SetTrigger("AtkTrigger");
    }

    public Vector2 MoveAnimationDirection {
        get => playerFaceDirection;
        set {
            playerFaceDirection = new Vector2(value.x, value.y);
            animator.SetFloat("x", value.x);
            animator.SetFloat("y", value.y);
        }
    }

    public bool TakeDamageAnimation {
        set => animator.SetTrigger("TakeDamageTrigger");
    }

    public bool EatAnimation {
        set => animator.SetTrigger("EatTrigger");
    }

    // ================================================
    // ============= IPlayerStatusAdapter =============
    // ================================================


    [field: SerializeField] public IntVariable playerLevel{get;private set;}
    [field: SerializeField] public IntVariable playerMaxHealth{get;private set;}
    [field: SerializeField] public IntVariable playerCurrentHealth{get;private set;}
    [field: SerializeField] public IntVariable playerMaxMuscle{get;private set;}
    [field: SerializeField] public IntVariable playerCurrentMuscle{get;private set;}
    [field: SerializeField] public IntVariable playerBasicAttackPower{get;private set;}
    [field: SerializeField] public IntVariable playerDefencePower{get;private set;}
    [field: SerializeField] public IntVariable playerExperience{get;private set;}

    [field: SerializeField] public WeaponSO EquipWeapon { get; private set; }
    [field: SerializeField] public ShieldSO EquipShield { get; private set; }


    // プレイヤーのHPを変更するメソッド
    public void ChangePlayerCurrentHealth(int value) {
        if (value < 0) {
            Debug.Log("PlayerのHPが0以下になりました。");
            playerCurrentHealth.Value = 0;
            OnPlayerHPChanged.Raise();
            return;
        } else if (value > playerMaxHealth.Value) {
            Debug.Log("PlayerのHPが最大値を超えました。");
            playerCurrentHealth.Value = playerMaxHealth.Value;
            OnPlayerHPChanged.Raise();
            return;
        } else {
            playerCurrentHealth.Value = value;
            OnPlayerHPChanged.Raise();
        }
    }

    // プレイヤーの最大HPを変更するメソッド
    public void ChangePlayerMaxHealth(int value) {
        playerMaxHealth.Value = value;
        playerCurrentHealth.Value = value;
        OnPlayerHPChanged.Raise();
    }

    // プレイヤーのレベルを変更するメソッド
    public void ChangePlayerLevel(int value) {
        if (playerLevel.Value <= 1) return;
        playerLevel.Value = value;
        OnPlayerLevelChanged.Raise();
    }

    // プレイヤーの経験値を変更するメソッド
    public void ChangePlayerExperience(int value) {
        playerExperience.Value = value;

        // レベルアップ判定
        if (playerExperience.Value >= DungeonConstants.necessarryExp[playerLevel.Value + 1]) {
            playerLevel.Value++;
        }
    }

    public void ChangePlayerMaxMuscle(int value) {
        playerMaxMuscle.Value += value;
        playerCurrentMuscle.Value += value;
    }

    public void ChangePlayerCurrentMuscle(int value) {
        playerCurrentMuscle.Value += value;
    }


    

    public int level {
        get => _currentLv;
        set {
            _currentLv = value;
            // playerStatusDataLogic.LevelUp();                                        
            // OnLvChanged?.Invoke();
        }
    }

    public int MaxHealth {
        get => _maxHp;
        set {
            _maxHp = value;
            //OnHealthChanged?.Invoke();
        }
    }
    public int health {
        get => _currentHp;
        set {
            _currentHp = value > MaxHealth ? MaxHealth : value;
            //OnHealthChanged?.Invoke();
        }
    }
    public int Experience {
        get => _totalExp;
        set {
            _totalExp += value;
            if (_totalExp >= DungeonConstants.necessarryExp[level + 1]) {
                level++;
            }
        }
    }

    // ================================================
    // ================ Methods ======================
    // ================================================
    public void InitializePlayer() {
        SetPlayerStatusDefault();
        //Id = CharacterManager.GetUniqueID();
        CharacterManager.i.AddCharacter(this);
        //Position.SetValue(new Vector2Int((int)transform.position.x, (int)transform.position.y));
        playerAnimLogic = new PlayerAnimLogic(this);
        playerMoveLogic = new PlayerMoveLogic(this, playerAnimLogic, this, OnPlayerStateComplete);
        playerAttackLogic = new PlayerAttackLogic(playerAnimLogic, this, this, this, OnPlayerStateComplete, this);
        playerStatusDataLogic = new PlayerStatusDataLogic(this, this, this, this);
        createMessageLogic = new CreateMessageLogic();
        playerInventory = new PlayerInventory(OnPlayerStateComplete);
        // Event subscriptions
        userInput.onAttack.AddListener(playerAttackLogic.Attack);
        userInput.onMoveInput.AddListener(playerMoveLogic.MoveByInput);
        MessageBus.Instance.Subscribe(DungeonConstants.GetExp, playerStatusDataLogic.GetExp);
    }

    private void SetPlayerStatusDefault() {
        if (resetStatus) {
            ChangePlayerMaxHealth(15);
            ChangePlayerMaxMuscle(8);
            ChangePlayerLevel(1);
            ChangePlayerCurrentHealth(playerMaxHealth.Value);
            ChangePlayerCurrentMuscle(playerMaxMuscle.Value);
            playerBasicAttackPower.Value = 0;
            playerDefencePower.Value = 0;
        }
    }

    public void TakeDamage(int damage, string dealerName) {
        playerStatusDataLogic.TakeDamage(damage, dealerName);
    }

    // ================================================
    // ============== IEffectReceiver =================
    // ================================================ 
    public void ApplyEffect(EffectSO effect) {
        effect.ApplyEffect(this);
    }

    public void Heal(int amount) {
        playerAnimLogic.SetEatAnimation();

        //体力MAXの場合
        if (playerMaxHealth.Value == playerCurrentHealth.Value) {
            ChangePlayerMaxHealth(playerMaxHealth.Value + 1);
            MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateMaxHpUpMessage(1));
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, playerMaxHealth.Value - playerCurrentHealth.Value);
        ChangePlayerCurrentHealth(healAmount);
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateHealMessage(healAmount, Name.Value));
    }

    public IPlayerStatusAdapter InformPlayerStatusAdapter() {
        return this;
    }
}
