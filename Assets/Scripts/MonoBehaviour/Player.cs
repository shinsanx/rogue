using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour, IAnimationAdapter, IDamageable, IPlayerStatusAdapter, IObjectData {
    // === Serialized Fields ===
    [SerializeField] private RandomDungeonWithBluePrint.RandomMapTest randomMapTest;
    [SerializeField] private DungeonEventManager dungeonEventManager;
    [SerializeField] private Animator animator;
    [SerializeField] private UserInput userInput;

    // === Private Fields ===
    private PlayerMoveLogic playerMoveLogic;
    private PlayerAttackLogic playerAttackLogic;
    private TileLogic tileLogic;
    private PlayerAnimLogic playerAnimLogic;
    private PlayerStatusDataLogic playerStatusDataLogic;
    private CreateMessageLogic createMessageLogic;

    private Vector2Int playerPosition;
    private Vector2 playerFaceDirection;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);

    private int _currentHp;
    private int _maxHp;
    private int _currentLv;
    private int _totalExp;    

    // === Events ===
    // CharacterManagerで実装。
    public event Action<IObjectData> OnObjectUpdated;

    // === Properties ===

    // IObjectData
    public int Id { get; set; }
    public string Name { get; set; }
    string IObjectData.Type { get; set; }
    int IObjectData.RoomNum { get; set; }

    public Vector2Int Position {
        get => playerPosition;
        set {
            playerPosition = value;
            transform.DOMove(value.ToVector2() + moveOffset, 0.3f).SetEase(Ease.Linear);
            OnObjectUpdated.Invoke(this);
        }
    }
    


    // IAnimationAdapter
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

    // IStatusAdapter
    public int level {
        get => _currentLv;
        set {
            _currentLv = value;
            playerStatusDataLogic.LevelUp();
            if (_currentLv >= 2) {
                MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.LvUppedMessage(Name, _currentLv));
            }
            MessageBus.Instance.Publish(DungeonConstants.UpdateLvText, this);
        }
    }

    public int MaxHealth {
        get => _maxHp;
        set {
            _maxHp = value;
            MessageBus.Instance.Publish(DungeonConstants.UpdateHPText, this);
        }
    }

    public int health {
        get => _currentHp;
        set {
            _currentHp = value > MaxHealth ? MaxHealth : value;
            MessageBus.Instance.Publish(DungeonConstants.UpdateHPText, this);
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

    // === Unity Methods ===
    private void OnEnable() {
        Id = CharacterManager.GetUniqueID();
    }

    private void Start() {
        InitializePlayer();
    }

    // === Methods ===
    private void InitializePlayer() {
        playerPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        tileLogic = new TileLogic();
        playerAnimLogic = new PlayerAnimLogic(this);
        playerMoveLogic = new PlayerMoveLogic(this, playerAnimLogic);
        playerAttackLogic = new PlayerAttackLogic(playerAnimLogic, this, this, this);
        playerStatusDataLogic = new PlayerStatusDataLogic(this, this, this);
        createMessageLogic = new CreateMessageLogic();

        // Event subscriptions
        userInput.onAttack.AddListener(playerAttackLogic.Attack);
        userInput.onMoveInput.AddListener(playerMoveLogic.MoveByInput);
        randomMapTest.onFieldUpdate.AddListener(tileLogic.UpdateFieldInformation);
        playerStatusDataLogic.SetStatusDefault(this);
        playerStatusDataLogic.SetObjectDataDefault(this);
        CharacterManager.i.AddCharacter(this);
        MessageBus.Instance.Subscribe(DungeonConstants.GetExp, playerStatusDataLogic.GetExp);
    }

    public void TakeDamage(int damage, string dealerName) {
        playerStatusDataLogic.TakeDamage(damage, dealerName);
    }
}
