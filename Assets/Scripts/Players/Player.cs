using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Threading.Tasks;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour,  IDamageable, IPlayerStatusAdapter, IEffectReceiver {
    // === Serialized Fields ===            
    [SerializeField] private ObjectDataRuntimeSet objectDataSet;    

    // === Private Fields ===    
    private PlayerMoveLogic playerMoveLogic;
    private PlayerAttackLogic playerAttackLogic;        
    private PlayerStatusDataLogic playerStatusDataLogic;
    [SerializeField]private CreateMessageLogic createMessageLogic;
    public ObjectData playerObjectData;
    [SerializeField] private CurrentSelectedObjectSO currentSelectedObjectSO;
    [SerializeField] private MessageEventChannelSO onMessageSend;    
    [SerializeField] private BoolVariable fixDiagonalInput;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] bool resetStatus = true;
    [SerializeField] private FloatVariable moveSpeed;
    [SerializeField] private BoolVariable dashInput;
    [SerializeField] private BoolVariable isTurnButtonLongPressed;
    [SerializeField] private BoolVariable zDashInput;
    private bool isMoving = false;
    public bool IsMoving() => isMoving;


    // ================================================
    // ==================== Events ====================
    // ================================================            

    [Header("イベント")]
    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerLevelChanged;

    // StatusUI.csで実装。
    [SerializeField] GameEvent OnPlayerHPChanged;

    // AttackLogic, MoveLogic, Inventoryで実装。
    [SerializeField] GameEvent OnPlayerStateComplete;    

    // AnimationAdapterで実装。
    [SerializeField] GameEvent OnPlayerTakeDamage;
    [SerializeField] GameEvent OnPlayerDirectionChanged;
    [SerializeField] GameEvent OnPlayerAttack;
    [SerializeField] GameEvent OnPlayerEat;
    [SerializeField] ItemEventChannelSO OnItemPicked; //PlayerMoveLogicで使用

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
    [field:SerializeField] public Vector2Variable playerFaceDirection{get;private set;}
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
    public void AddPlayerExperience(int value) {
        playerExperience.Value += value;

        // レベルアップ判定
        if (playerExperience.Value >= DungeonConstants.necessarryExp[playerLevel.Value + 1]) {
            playerLevel.Value++;
        }
    }

    public void ChangePlayerMaxMuscle(int value) {
        playerMaxMuscle.SetValue(value);
    }

    public void ChangePlayerCurrentMuscle(int value) {
        playerCurrentMuscle.SetValue(value);
    }

    // UserInputからのイベントを受け取るメソッド
    public async void PlayerMove(Vector2 direction) {
        if(isTurnButtonLongPressed.Value) {
            playerMoveLogic.ManualTurn(direction);
            return;
        }
        if(confusionTurn.Value > 0) {
            if(playerMoveLogic.RandomMove()) {
                confusionTurn.Value--;
            }
            return;
        }
        if(dashInput.Value) {
            moveSpeed.Value = 0.05f;
            playerMoveLogic.DashByInput(direction);
        } else if (zDashInput.Value) {
            moveSpeed.Value = 0.05f;
            await playerMoveLogic.ZDash(direction);
        } else {
            moveSpeed.Value = 0.3f;
            playerMoveLogic.MoveByInput(direction);
        }
    }
    public void PlayerAttack() { 
        if(confusionTurn.Value > 0) {
            if(playerAttackLogic.ConfusionAttack()) {
                confusionTurn.Value--;
            }
            return;
        }
        playerAttackLogic.Attack();
    }

    // オブジェクトの位置を変更するメソッド
    public void MovePosition() {        
        transform.DOMove(playerObjectData.Position.Value.ToVector2() + moveOffset, moveSpeed.Value)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isMoving = false;
            });
    }

    // OnAutoTurnInputから呼び出される
    public void AutoTurn() {
        playerMoveLogic.AutoTurn(playerObjectData.Position.Value);
    }

    // OnFootStepInputから呼び出される
    public void FootStep() {
        // todo: 足踏みのアニメーションを再生する
        //gameObject.GetComponent<Animator>().speed = 2f;
        // todo: 同じ部屋にEnemyがいる場合は足踏みスピードを遅くする
        // Playerのターンを終了する
        OnPlayerStateComplete.Raise();
    }
    

    // ================================================
    // ================ Methods ======================
    // ================================================
    public void InitializePlayer() {
        SetPlayerStatusDefault();
        playerObjectData.SetId(CharacterManager.GetUniqueID());                        
        playerMoveLogic = new PlayerMoveLogic(this, OnPlayerStateComplete, OnPlayerDirectionChanged, playerFaceDirection, OnItemPicked, currentSelectedObjectSO, fixDiagonalInput);
        playerAttackLogic = new PlayerAttackLogic(this, OnPlayerStateComplete, objectDataSet, playerFaceDirection, OnPlayerAttack, OnPlayerDirectionChanged);
        playerStatusDataLogic = new PlayerStatusDataLogic(this, createMessageLogic, onMessageSend);                
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
        OnPlayerTakeDamage.Raise();
    }

    // ================================================
    // ============== IEffectReceiver =================
    // ================================================ 
    [field: SerializeField] public IntVariable sleepTurn { get; set; }
    [field: SerializeField] public IntVariable confusionTurn { get; set; }

    public void ApplyEffect(EffectSO effect) {
        effect.ApplyEffect(this);
    }

    public void Heal(int amount) {
        OnPlayerEat.Raise();

        //体力MAXの場合
        if (playerMaxHealth.Value == playerCurrentHealth.Value) {
            ChangePlayerMaxHealth(playerMaxHealth.Value + 1);
            onMessageSend.RaiseEvent(createMessageLogic.CreateMaxHpUpMessage(1));
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, playerMaxHealth.Value - playerCurrentHealth.Value);
        ChangePlayerCurrentHealth(healAmount);
        onMessageSend.RaiseEvent(createMessageLogic.CreateHealMessage(healAmount, playerObjectData.Name.Value));
    }

    public void Equip(ItemSO item) {
        if (item is WeaponSO weapon) {
            EquipWeapon = weapon;
        } else if (item is ShieldSO shield) {
            EquipShield = shield;
        }
    }

    public void HandleItemPicked(bool success) {
        playerMoveLogic.HandleItemPicked(success);
    }

    public void MuscleUp(int amount) {
        ChangePlayerMaxMuscle(playerMaxMuscle.Value + amount);
        playerCurrentMuscle.Value += amount;

        onMessageSend.RaiseEvent(createMessageLogic.CreateMuscleUpMessage(amount));
    }

}
