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
    private StateMachine stateMachine;
    private Vector2Int playerPosition;
    private Vector2 playerFaceDirection;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);

    private int _currentHp;
    private int _maxHp;
    private int _currentLv;
    private int _totalExp;

    private bool isMoving = false;
    public bool IsMoving() => isMoving;


    // ================================================
    // ==================== Events ====================
    // ================================================    
    
    [SerializeField] VoidEventChannelSO CompletePlayerStateChannel;
    
    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerLevelChanged;

    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerHPChanged;

    // ================================================
    // ================== ObjectData ==================
    // ================================================
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int RoomNum { get; set; }

    public Vector2Int Position {
        get => playerPosition;
        set {
            playerPosition = value;
            isMoving = false; //本当はtrueにする。デバッグのためにfalseにしている。
            transform.DOMove(value.ToVector2() + moveOffset, 0.3f)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    isMoving = false;
                    OnObjectUpdated?.Invoke(this);
                });
        }
    }
    
    // CharacterManagerのAddCharacterで実装。
    public event Action<IObjectData> OnObjectUpdated; // オブジェクトの更新を渡すイベント

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

    public IntVariable playerId;
    public IntVariable playerLevel;
    public IntVariable playerMaxHealth;
    public IntVariable playerCurrentHealth;
    public IntVariable playerMaxMuscle;
    public IntVariable playerCurrentMuscle;
    public IntVariable playerBasicAttackPower;
    public IntVariable playerDefencePower;
    public IntVariable playerExperience;

    public WeaponSO EquipWeapon { get; set; }
    public ShieldSO EquipShield { get; set; }


    // プレイヤーのHPを変更するメソッド
    public void ChangePlayerCurrentHealth(int value){
        if(value < 0){
            Debug.Log("PlayerのHPが0以下になりました。");
            playerCurrentHealth.Value = 0;
            OnPlayerHPChanged.Raise();
        } else if(value > playerMaxHealth.Value){
            Debug.Log("PlayerのHPが最大値を超えました。");
            playerCurrentHealth.Value = playerMaxHealth.Value;
            OnPlayerHPChanged.Raise();
        } else{            
            playerCurrentHealth.Value = value;
            OnPlayerHPChanged.Raise();
        }
    }

    // プレイヤーの最大HPを変更するメソッド
    public void ChangePlayerMaxHealth(int value){        
        playerMaxHealth.Value = value;
        playerCurrentHealth.Value = value;
        OnPlayerHPChanged.Raise();
    }

    // プレイヤーのレベルを変更するメソッド
    public void ChangePlayerLevel(int value){
        if(playerLevel.Value <= 1) return;        
        playerLevel.Value += value;
        OnPlayerLevelChanged.Raise();
    }

    // プレイヤーの経験値を変更するメソッド
    public void ChangePlayerExperience(int value){
        playerExperience.Value += value;        

        // レベルアップ判定
        if(playerExperience.Value >= DungeonConstants.necessarryExp[playerLevel.Value + 1]){
            playerLevel.Value++;                        
        }
    }

    public void ChangePlayerMaxMuscle(int value){
        playerMaxMuscle.Value += value;
        playerCurrentMuscle.Value += value;
    }

    public void ChangePlayerCurrentMuscle(int value){
        playerCurrentMuscle.Value += value;
    }
    

    // Additional properties
    public int Level { get; set; }
    public int MaxSatiety { get; set; }
    public int Satiety { get; set; }
    public int MaxMuscle { get; set; }
    public int Muscle { get; set; }
    public int BasicAttackPower { get; set; }
    public int DefencePower { get; set; }

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
        Id = CharacterManager.GetUniqueID();
        CharacterManager.i.AddCharacter(this);
        playerPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        playerAnimLogic = new PlayerAnimLogic(this);
        playerMoveLogic = new PlayerMoveLogic(this, playerAnimLogic, this, CompletePlayerStateChannel);
        playerAttackLogic = new PlayerAttackLogic(playerAnimLogic, this, this, this, CompletePlayerStateChannel, this);
        playerStatusDataLogic = new PlayerStatusDataLogic(this, this, this, this);
        createMessageLogic = new CreateMessageLogic();
        playerInventory = new PlayerInventory(CompletePlayerStateChannel);
        // Event subscriptions
        userInput.onAttack.AddListener(playerAttackLogic.Attack);
        userInput.onMoveInput.AddListener(playerMoveLogic.MoveByInput);
        playerStatusDataLogic.SetStatusDefault(this);
        playerStatusDataLogic.SetObjectDataDefault(this);
        MessageBus.Instance.Subscribe(DungeonConstants.GetExp, playerStatusDataLogic.GetExp);
        stateMachine = GameAssets.i.stateMachine;

    }

    private void SetPlayerStatusDefault(){
        ChangePlayerMaxHealth(15);        
        ChangePlayerMaxMuscle(8);
        ChangePlayerLevel(1);
        ChangePlayerCurrentHealth(playerMaxHealth.Value);
        ChangePlayerCurrentMuscle(playerMaxMuscle.Value);
        playerBasicAttackPower.Value = 0;
        playerDefencePower.Value = 0;
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
        if(playerMaxHealth.Value == playerCurrentHealth.Value){
            ChangePlayerMaxHealth(playerMaxHealth.Value + 1);
            MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateMaxHpUpMessage(1));
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, playerMaxHealth.Value - playerCurrentHealth.Value);
        ChangePlayerCurrentHealth(healAmount);
        MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.CreateHealMessage(healAmount, Name));
    }

    public IPlayerStatusAdapter InformPlayerStatusAdapter() {
        return this;
    }
}
