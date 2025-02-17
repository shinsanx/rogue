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
    private PlayerInventory playerInventory;
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

    // ================================================
    // ==================== Events ====================
    // ================================================
    // CharacterManagerのAddCharacterで実装。
    public event Action<IObjectData> OnObjectUpdated; // オブジェクトの更新を渡すイベント

    // StatusUIで実装。
    public event Action<int, int> OnHealthChanged; // 現在のHPと最大HPを渡すイベント
    public event Action<int> OnLvChanged; // レベルを渡すイベント

    // ================================================
    // ================== Properties ==================
    // ================================================
    public int Id { get; set; }
    public string Name { get; set; }
    string IObjectData.Type { get; set; }
    int IObjectData.RoomNum { get; set; }

    public Vector2Int Position {
        get => playerPosition;
        set {
            playerPosition = value;
            isMoving = true;
            transform.DOMove(value.ToVector2() + moveOffset, 0.3f)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    isMoving = false;
                    OnObjectUpdated?.Invoke(this);
                });
        }
    }

    public bool IsMoving() => isMoving;

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

    // ================================================
    // ============= IPlayerStatusAdapter =============
    // ================================================

    public WeaponSO EquipWeapon { get; set; }
    public ShieldSO EquipShield { get; set; }

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
            playerStatusDataLogic.LevelUp();
            if (_currentLv >= 2) {
                MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.LvUppedMessage(Name, _currentLv));
            }
            OnLvChanged?.Invoke(_currentLv);
        }
    }

    public int MaxHealth {
        get => _maxHp;
        set {
            _maxHp = value;
            OnHealthChanged?.Invoke(_currentHp, _maxHp);
        }
    }
    public int health {
        get => _currentHp;
        set {
            _currentHp = value > MaxHealth ? MaxHealth : value;
            OnHealthChanged?.Invoke(_currentHp, _maxHp);
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
        Id = CharacterManager.GetUniqueID();
        CharacterManager.i.AddCharacter(this);
        playerPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        playerAnimLogic = new PlayerAnimLogic(this);
        playerMoveLogic = new PlayerMoveLogic(this, playerAnimLogic, this);
        playerAttackLogic = new PlayerAttackLogic(playerAnimLogic, this, this, this);
        playerStatusDataLogic = new PlayerStatusDataLogic(this, this, this);
        createMessageLogic = new CreateMessageLogic();
        playerInventory = new PlayerInventory();
        // Event subscriptions
        userInput.onAttack.AddListener(playerAttackLogic.Attack);
        userInput.onMoveInput.AddListener(playerMoveLogic.MoveByInput);
        playerStatusDataLogic.SetStatusDefault(this);
        playerStatusDataLogic.SetObjectDataDefault(this);
        MessageBus.Instance.Subscribe(DungeonConstants.GetExp, playerStatusDataLogic.GetExp);
        stateMachine = GameAssets.i.stateMachine;

        // ActionEventManagerで使用するためにセット
        ActionEventManager.OnPlayerActionComplete += () => {
            stateMachine.SetState(GameAssets.i.enemyState);
        };
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
        health += amount;
    }
}
