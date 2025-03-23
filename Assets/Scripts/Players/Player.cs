using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

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

    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] bool resetStatus = true;

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
    public void PlayerMove(Vector2 direction) {        
        playerMoveLogic.MoveByInput(direction);
    }
    public void PlayerAttack() {        
        playerAttackLogic.Attack();
    }

    // オブジェクトの位置を変更するメソッド
    public void MovePosition() {
        transform.DOMove(playerObjectData.Position.Value.ToVector2() + moveOffset, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                isMoving = false;
            });
    }
    

    // ================================================
    // ================ Methods ======================
    // ================================================
    public void InitializePlayer() {
        SetPlayerStatusDefault();
        playerObjectData.SetId(CharacterManager.GetUniqueID());                        
        playerMoveLogic = new PlayerMoveLogic(this, OnPlayerStateComplete, OnPlayerDirectionChanged, playerFaceDirection, OnItemPicked, currentSelectedObjectSO);
        playerAttackLogic = new PlayerAttackLogic(this, OnPlayerStateComplete, objectDataSet, playerFaceDirection, OnPlayerAttack);
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

    public void HandleItemPicked(bool success) {
        playerMoveLogic.HandleItemPicked(success);
    }

}
