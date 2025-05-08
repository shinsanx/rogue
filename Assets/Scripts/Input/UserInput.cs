using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System;

public class UserInput : MonoBehaviour {
    // インスペクターで設定                
    Vector2 inputVector;
    [SerializeField] private BoolVariable isMoveButtonLongPrresed;
    [SerializeField] private BoolVariable isTurnButtonLongPressed;
    [SerializeField] private BoolVariable canHandleInput;
    [SerializeField] private BoolVariable PlayerCanMove;
    // === unified move‑handling ===
    [SerializeField] private BoolVariable CanMove;   // Player が移動可能になると true
    private int horizontal = 0;                      // ‑1(左) / 0 / 1(右)
    private int vertical = 0;                      // ‑1(下) / 0 / 1(上)
    private enum Axis { None, Horizontal, Vertical }
    private Axis lastAxisChanged = Axis.None;        // 直近に変化した軸
    private Coroutine moveRepeatCoroutine = null;    // 移動ループ用
    private bool awaitingFirstStep = false;      // 単発入力 → 初回 1 歩をまだ送っていない
    private Coroutine firstStepCoroutine = null;


    // イベント
    public GameEvent OnAttackInput;
    public Vector2EventChannelSO OnMoveInput;
    public Action<Vector2> OnMoveInputAction; //移動
    public GameEvent OnMenuOpenInput;
    public GameEvent OnMenuCloseInput;
    public Vector2EventChannelSO OnNavigateInput;
    public GameEvent OnSubmitInput;
    public GameEvent OnAutoTurnInput;
    public GameEvent OnFootStepInput;


    [SerializeField] private BoolVariable fixDiagonalInput;
    [SerializeField] private BoolVariable dashInput;
    [SerializeField] private BoolVariable zDashInput;
    [SerializeField] private FloatVariable moveSpeed;
    private bool isFootStep = false;

    // 「押しっぱなしで連続移動」を始めるまで／次の1歩までのポーズ (秒)
    [SerializeField, Tooltip("キーを押し続けたとき2歩目以降が出るまでの待ち時間")]
    private float repeatDelay = 0.15f;

    // 2 軸が「ほぼ同時」に押されたと判定する許容時間 (秒)
    [SerializeField, Tooltip("この時間以内に上下左右が同時に押されると斜め移動を許可")]
    private float simultaneousThreshold = 0.1f;

    // 押下されたフレームの時刻
    private float horizontalPressedAt = 0f;
    private float verticalPressedAt = 0f;

    [SerializeField] private InputActionAsset inputActionAsset; //アクションマップ
    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    private void Start() {
        // ActionMapの取得
        playerActionMap = inputActionAsset.FindActionMap("Player", true);
        uiActionMap = inputActionAsset.FindActionMap("UI", true);

        // 初期状態ではPlayer ActionMapを有効化し、UI ActionMapを無効化
        playerActionMap.Enable();
        uiActionMap.Disable();
    }

    public void OnToggleActionMap() {
        if (playerActionMap.enabled) {
            playerActionMap.Disable();
            uiActionMap.Enable();
        } else if (MenuManager.Instance.activeMenus.Count == 0) {
            playerActionMap.Enable();
            uiActionMap.Disable();
        }
    }

    public void OnEnableActionMap() {
        playerActionMap.Enable();
        uiActionMap.Disable();
    }

    public void OnDisableActionMap() {
        playerActionMap.Disable();
        uiActionMap.Enable();
    }


    // ================================================
    // ==================== Playerの入力 ====================
    // ================================================

    // 移動の入力は NewInputSystem コールバック一本化
    public void OnMove(InputAction.CallbackContext context) {
        // performed / canceled だけで十分
        if (!(context.performed || context.canceled)) return;

        // ── 1) 現在の生入力 ─────────────────────────
        Vector2 raw = context.ReadValue<Vector2>();
        int newH = Mathf.RoundToInt(raw.x);
        int newV = Mathf.RoundToInt(raw.y);

        bool changed = false;
        if (newH != horizontal) {
            horizontal = newH;
            if (newH != 0) horizontalPressedAt = Time.time;
            if (newH != 0) lastAxisChanged = Axis.Horizontal;
            changed = true;
        }
        if (newV != vertical) {
            vertical = newV;
            if (newV != 0) verticalPressedAt = Time.time;
            if (newV != 0) lastAxisChanged = Axis.Vertical;
            changed = true;
        }

        if (!changed) return; // 方向に全く変化なし


        // ── 2) すべてキーが離された場合 ─────────────────
        if (horizontal == 0 && vertical == 0) {
            // 全停止
            if (moveRepeatCoroutine != null) {
                StopCoroutine(moveRepeatCoroutine);
                moveRepeatCoroutine = null;
            }
            if (awaitingFirstStep && firstStepCoroutine != null) {
                StopCoroutine(firstStepCoroutine);
                awaitingFirstStep = false;
            }
            return;
        }

        // ── 3) まだ移動ループが始まっていないなら、まず「初回 1 歩」用のキューを準備 ──
        if (!awaitingFirstStep && moveRepeatCoroutine == null) {
            awaitingFirstStep = true;
            firstStepCoroutine = StartCoroutine(FirstStepCoroutine());
        }
    }


    // 押下直後の "初回 1 歩だけ" を送るための小さな待機
    private IEnumerator FirstStepCoroutine() {
        // 次フレームまで待機して、ほぼ同時押しを拾う
        yield return null;

        // Player 側がまだ移動中なら完了を待つ
        while (!CanMove.Value) yield return null;

        Vector2Int dir = ComputeStepDirection();
        if (dir != Vector2Int.zero) {
            OnMoveInputAction?.Invoke(dir);
        }

        awaitingFirstStep = false;
        firstStepCoroutine = null;

        // キーが押下されたままなら連続移動ループを開始
        if (horizontal != 0 || vertical != 0) {
            moveRepeatCoroutine = StartCoroutine(MoveRepeat(true)); // true = 最初の待機を挟む
        }
    }

    // 連続移動コルーチン
    // initialPause == true の場合、最初に repeatDelay を空けてからループ
    private IEnumerator MoveRepeat(bool initialPause = false) {
        if (initialPause) yield return new WaitForSeconds(repeatDelay);

        while (horizontal != 0 || vertical != 0) {
            // Player 側の移動完了を待つ
            while (!CanMove.Value) yield return null;

            Vector2Int dir = ComputeStepDirection();
            if (dir != Vector2Int.zero) {
                OnMoveInputAction?.Invoke(dir);
            }

            // 次のステップまでポーズ
            yield return new WaitForSeconds(repeatDelay);
        }
        moveRepeatCoroutine = null;
    }

    // 現在保持している水平・垂直入力から “今回 1 マスだけ” の方向を決定
    private Vector2Int ComputeStepDirection() {
        // 斜め判定
        if (horizontal != 0 && vertical != 0) {
            // ほぼ同時押しなら斜め移動を許可
            if (Mathf.Abs(horizontalPressedAt - verticalPressedAt) <= simultaneousThreshold) {
                return new Vector2Int(horizontal, vertical);
            }

            // 片方後押しなら最後に変化した軸を優先
            return lastAxisChanged == Axis.Horizontal
                ? new Vector2Int(horizontal, 0)
                : new Vector2Int(0, vertical);
        }

        // 単一軸押し
        return new Vector2Int(horizontal, vertical);
    }

    //入力を取得する
    private Vector2 GetCurrentInput() {
        Vector2 result = Gamepad.current != null ?
                Gamepad.current.leftStick.ReadValue() :
                Keyboard.current != null ? new Vector2(
                    (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                    (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)) :
                Vector2.zero;
        return result;
    }

    // 攻撃 keyboard:Space
    public void OnAttack(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        OnAttackInput.Raise();
    }


    // 斜め移動を固定する keyboard:O
    public void OnFixDiagonalInput(InputAction.CallbackContext context) {
        if (context.started) fixDiagonalInput.Value = true;
        if (context.canceled) fixDiagonalInput.Value = false;
    }

    // ダッシュ待機する keyboard:K
    public void OnDash(InputAction.CallbackContext context) {
        if (context.started) dashInput.Value = true;
        if (context.canceled) dashInput.Value = false;
    }

    // zダッシュ待機する keyboard:Z
    public void OnZDash(InputAction.CallbackContext context) {
        if (context.started) zDashInput.Value = true;
        if (context.canceled) zDashInput.Value = false;
    }

    // 振り向く keyboard:U
    public void OnTurn(InputAction.CallbackContext context) {
        // 押している時間が0.5秒未満ならOnAutoTurnInputを呼び出す
        // 押している時間が0.5秒以上ならisTurnButtonLongPressedをtrueにする
        if (context.started) {
            isTurnButtonLongPressed.Value = true;
            StartCoroutine(CheckTurnButtonHoldTime());
        }
        if (context.canceled) {
            isTurnButtonLongPressed.Value = false;
        }
    }

    // 振り向くボタンの長押しをチェック
    private IEnumerator CheckTurnButtonHoldTime() {
        yield return new WaitForSeconds(0.2f);

        // 0.5秒後もボタンが押されていれば長押し状態を維持
        // そうでなければ（既にキャンセルされていれば）自動回転を実行
        if (!isTurnButtonLongPressed.Value) {
            OnAutoTurnInput.Raise();
        }
    }

    // 足踏み keyboard K+Space
    public void OnFootStep(InputAction.CallbackContext context) {
        if (context.started) {
            isFootStep = true;
            FootStepContinuously();
        }
        if (context.canceled) {
            isFootStep = false;
        }
    }

    // 足踏み長押し処理
    private async void FootStepContinuously() {
        if (!isFootStep) return;
        while (isFootStep) {
            OnFootStepInput.Raise();
            await Task.Delay(50);
        }
    }


    // ================================================
    // ==================== UIの入力 ====================
    // ================================================

    /// <summary>
    /// メニューを開く際に呼び出す
    /// </summary>
    public void OnMenuOpen(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnMenuOpenInput.Raise();
    }

    /// <summary>
    /// メニューを閉じる際に呼び出す
    /// </summary>
    public void OnMenuClose(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnMenuCloseInput.Raise();
    }

    public void OnNavigate(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        Vector2 navigateVector = context.ReadValue<Vector2>();
        if (navigateVector.magnitude == 0) return;
        OnNavigateInput.RaiseEvent(navigateVector);
    }

    public void OnSubmit(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        OnSubmitInput.Raise();
    }

}
