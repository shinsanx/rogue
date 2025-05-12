using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour, IDamageable, IPlayerStatusAdapter, IEffectReceiver {
    // === Serialized Fields ===            
    [SerializeField] private ObjectDataRuntimeSet objectDataSet;
    [SerializeField] private UserInput userInput;

    // === Private Fields ===
    private PlayerMoveLogic playerMoveLogic;
    private PlayerAttackLogic playerAttackLogic;
    private PlayerStatusDataLogic playerStatusDataLogic;
    [SerializeField] private CreateMessageLogic createMessageLogic;
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
    [SerializeField] private BoolVariable CanMove;
    //private bool isMoving = false;
    //public bool IsMoving() => isMoving;

    //行動ゲージ関連
    private int threshold = 100; //行動の閾値
    [SerializeField] private IntVariable timeGage;
    [SerializeField] private IntVariable playerActionRate;
    public int actionRate {
        get => playerActionRate.Value;
        set => playerActionRate.Value = value;
    }


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

    [field: SerializeField] public IntVariable playerLevel { get; private set; }
    [field: SerializeField] public IntVariable playerMaxHealth { get; private set; }
    [field: SerializeField] public IntVariable playerCurrentHealth { get; private set; }
    [field: SerializeField] public IntVariable playerMaxMuscle { get; private set; }
    [field: SerializeField] public IntVariable playerCurrentMuscle { get; private set; }
    [field: SerializeField] public IntVariable playerBasicAttackPower { get; private set; }
    [field: SerializeField] public IntVariable playerDefencePower { get; private set; }
    [field: SerializeField] public IntVariable playerExperience { get; private set; }
    [field: SerializeField] public Vector2Variable playerFaceDirection { get; private set; }
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

        // palyerMaxMuscleを超えないようにする
        playerCurrentMuscle.SetValue(Mathf.Min(value, playerMaxMuscle.Value));
    }

    // UserInputからのイベントを受け取るメソッド
    public async void PlayerMove(Vector2 direction) {
        moveSpeed.Value = 0.3f;
        if (isTurnButtonLongPressed.Value) {
            playerMoveLogic.ManualTurn(direction);
            return;
        }
        if (isConfusion.Value) {
            // RandomMoveが成功したときのみ終了
            if (playerMoveLogic.RandomMove()) return;
        }
        if (dashInput.Value) {
            moveSpeed.Value = 0.05f;
            playerMoveLogic.DashByInput(direction);
        } else if (zDashInput.Value) {
            moveSpeed.Value = 0.05f;
            await playerMoveLogic.ZDash(direction);
        } else {
            playerMoveLogic.MoveByInput(direction);
        }
    }
    public void PlayerAttack() {
        if (isConfusion.Value) {
            if (playerAttackLogic.ConfusionAttack()) {
            }
            return;
        }
        playerAttackLogic.Attack();
    }

    // オブジェクトの位置を変更するメソッド
    public void MovePosition() {
        // ① これから動く “目標グリッド座標” を一旦キャッシュ
        Vector2 targetGridPos = playerObjectData.Position.Value.ToVector2();
        OnPlayerStateComplete.Raise();        // ★行動完了をここで 1 回だけ通知


        transform.DOMove(targetGridPos + moveOffset, moveSpeed.Value)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                // ② 実際に動き終わったワールド座標を逆変換して
                //    ObjectData に書き戻すだけで OK
                Vector2Int snapped = Vector2Int.RoundToInt(transform.position - (Vector3)moveOffset);
                playerObjectData.Position.SetValue(snapped);

                CanMove.Value = true;                 // 移動解除
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
        userInput.OnMoveInputAction += PlayerMove;
        SetPlayerStatusDefault();
        //playerObjectData.SetId(CharacterManager.GetUniqueID());
        playerMoveLogic = new PlayerMoveLogic(CanMove, playerObjectData, OnPlayerStateComplete, OnPlayerDirectionChanged, playerFaceDirection, OnItemPicked, currentSelectedObjectSO, fixDiagonalInput, TileManager.i);
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
    [field: SerializeField] public BoolVariable isConfusion { get; set; }
    [field: SerializeField] public BoolVariable isSleeping { get; set; }

    public void Heal(int amount, int maxUpAmount) {
        OnPlayerEat.Raise();

        //体力MAXの場合
        if (playerMaxHealth.Value == playerCurrentHealth.Value) {
            ChangePlayerMaxHealth(playerMaxHealth.Value + maxUpAmount);
            onMessageSend.RaiseEvent(createMessageLogic.CreateMaxHpUpMessage(maxUpAmount));
            return;
        }
        int healAmount = Mathf.Clamp(amount, 0, playerMaxHealth.Value - playerCurrentHealth.Value);
        ChangePlayerCurrentHealth(healAmount);
        onMessageSend.RaiseEvent(createMessageLogic.CreateHealMessage(healAmount, playerObjectData.Name.Value));
    }

    public void Equip(BaseItemSO item) {
        if (item is WeaponSO weapon) {
            EquipWeapon = weapon;
        } else if (item is ShieldSO shield) {
            EquipShield = shield;
        }
    }

    public void HandleItemPicked(bool success) {
        playerMoveLogic.HandleItemPicked(success);
    }

    // ちからの最大値を上げる
    public void MuscleUp(int amount) {
        ChangePlayerMaxMuscle(playerMaxMuscle.Value + amount);
        playerCurrentMuscle.Value += amount;

        onMessageSend.RaiseEvent(createMessageLogic.CreateMuscleUpMessage(amount));
    }

    // ちからを全回復する
    public void MuscleHeal() {
        ChangePlayerCurrentMuscle(playerMaxMuscle.Value);
        onMessageSend.RaiseEvent(createMessageLogic.CreateMuscleHealMessage());
    }

    public void TakePoison() {
        ChangePlayerCurrentMuscle(playerCurrentMuscle.Value - 1);
        TakeDamage(5, "");
    }


    // ================================================
    // ============== IEffectReceiver =============
    // ================================================
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
            Debug.Log(instance + " tick");
            instance.Tick();
            if (instance.IsExpired) {
                instance.EndEffect();
                activeEffects.Remove(instance);
            }
        }
    }

}
