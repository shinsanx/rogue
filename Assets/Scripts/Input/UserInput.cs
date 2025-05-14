using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System;

/// <summary>
/// プレイヤー／UI の入力をまとめて扱うコンポーネント.
/// 斜め移動・先行入力バッファ対応版🏃‍♀️
/// </summary>
public class UserInput : MonoBehaviour {
    // ─────────────────────────────
    //  インスペクタ設定
    // ─────────────────────────────
    [Header("Move Flags")]
    [SerializeField] private BoolVariable CanMove;                 // Player が移動可能
    [SerializeField] private float repeatDelay = 0.3f;             // 長押し間隔 (秒)

    [Header("Action Asset")]
    [SerializeField] private InputActionAsset inputActionAsset;    // Input System アセット

    // ゲーム固有フラグなど
    [SerializeField] private BoolVariable isTurnButtonLongPressed;
    [SerializeField] private BoolVariable fixDiagonalInput;
    [SerializeField] private BoolVariable dashInput;
    [SerializeField] private BoolVariable zDashInput;

    // ─────────────────────────────
    //  内部状態
    // ─────────────────────────────
    private int horizontal = 0;            // -1 / 0 / +1
    private int vertical = 0;            // -1 / 0 / +1
    private Coroutine moveRepeatCoroutine = null;
    private bool awaitingFirstStep = false;
    private Coroutine firstStepCoroutine = null;

    private Vector2Int queuedDir = Vector2Int.zero; // ★ 先行入力バッファ

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    // ─────────────────────────────
    //  イベント (必要に応じて Hook)
    // ─────────────────────────────
    public GameEvent OnAttackInput;
    public GameEvent OnMenuOpenInput;
    public GameEvent OnMenuCloseInput;
    public Vector2EventChannelSO OnNavigateInput;
    public GameEvent OnSubmitInput;
    public GameEvent OnAutoTurnInput;
    public GameEvent OnFootStepInput;

    public event Action<Vector2> OnMoveInputAction; // 多重登録防止に event

    // ─────────────────────────────
    //  Unity ライフサイクル
    // ─────────────────────────────
    private void Start() {
        playerActionMap = inputActionAsset.FindActionMap("Player", true);
        uiActionMap = inputActionAsset.FindActionMap("UI", true);

        playerActionMap.Enable();
        uiActionMap.Disable();
    }

    // ─────────────────────────────
    //  ActionMap 切り替え
    // ─────────────────────────────
    public void OnToggleActionMap() {
        if (playerActionMap.enabled) {
            playerActionMap.Disable();
            uiActionMap.Enable();
        } else if (MenuManager.Instance.activeMenus.Count == 0) {
            playerActionMap.Enable();
            uiActionMap.Disable();
        }
    }

    // ─────────────────────────────
    //  Player Move
    // ─────────────────────────────
    public void OnMove(InputAction.CallbackContext context) {
        // performed = 値が変わった瞬間にも呼ばれる
        // canceled  = 全キー離し
        if (!(context.performed || context.canceled)) return;

        // 1) デジタル化
        Vector2 raw = context.ReadValue<Vector2>();
        int newH = Mathf.RoundToInt(raw.x);
        int newV = Mathf.RoundToInt(raw.y);

        bool changed = false;

        if (newH != horizontal) { horizontal = newH; changed = true; }
        if (newV != vertical) { vertical = newV; changed = true; }

        if (!changed) return;

        // 2) すべて離した → 停止
        if (horizontal == 0 && vertical == 0) {
            queuedDir = Vector2Int.zero;                // バッファもクリア
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

        // 3) まだ移動中なら「先行入力」としてキュー
        if (!CanMove.Value || moveRepeatCoroutine != null) {
            Vector2Int newDir = ComputeStepDirection();

            // まだバッファ空 or 片軸しか入ってなければ上書き
            if (queuedDir == Vector2Int.zero || queuedDir.x == 0 || queuedDir.y == 0)
                queuedDir = newDir;
            return;
        }

        // 4) 初回 1 歩目を送る準備
        if (!awaitingFirstStep && moveRepeatCoroutine == null) {
            awaitingFirstStep = true;
            firstStepCoroutine = StartCoroutine(FirstStepCoroutine());
        }
    }

    // ─────────────────────────────
    //  押下直後の 1 歩目だけ送るコルーチン
    // ─────────────────────────────
    private IEnumerator FirstStepCoroutine() {
        yield return null;                     // 同時押し拾う
        while (!CanMove.Value) yield return null;

        awaitingFirstStep = false;
        firstStepCoroutine = null;

        // 押しっぱなら MoveRepeat 開始（delay 無し）
        if (horizontal != 0 || vertical != 0)
            moveRepeatCoroutine = StartCoroutine(MoveRepeat(false));
    }

    // ─────────────────────────────
    //  連続移動コルーチン
    // ─────────────────────────────
    private IEnumerator MoveRepeat(bool initialPause) {
        if (initialPause)
            yield return new WaitForSeconds(repeatDelay);

        while (horizontal != 0 || vertical != 0) {
            // 現移動完了待ち
            while (!CanMove.Value) yield return null;

            // ★ queuedDir があればそれを使い、
            //   無ければ現入力を読む
            Vector2Int dir = queuedDir != Vector2Int.zero
                           ? queuedDir
                           : ComputeStepDirection();

            // 送信後にクリア（次フレームまで温存）
            queuedDir = Vector2Int.zero;

            if (dir != Vector2Int.zero)
                OnMoveInputAction?.Invoke(dir);

            yield return new WaitForSeconds(repeatDelay);
        }
        moveRepeatCoroutine = null;
    }

    // ─────────────────────────────
    //  方向決定：縦横どちらも押されてたら斜め
    // ─────────────────────────────
    private Vector2Int ComputeStepDirection() {
        if (horizontal != 0 && vertical != 0)
            return new Vector2Int(horizontal, vertical);

        return new Vector2Int(horizontal, vertical);
    }

    // ─────────────────────────────
    //  その他アクション（攻撃 / ダッシュ など）
    // ─────────────────────────────
    public void OnAttack(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        OnAttackInput.Raise();
    }

    public void OnDash(InputAction.CallbackContext context) {
        if (context.started) dashInput.Value = true;
        if (context.canceled) dashInput.Value = false;
    }

    public void OnZDash(InputAction.CallbackContext context) {
        if (context.started) zDashInput.Value = true;
        if (context.canceled) zDashInput.Value = false;
    }

    public void OnTurn(InputAction.CallbackContext context) {
        if (context.started) {
            isTurnButtonLongPressed.Value = true;
            StartCoroutine(CheckTurnButtonHoldTime());
        }
        if (context.canceled) {
            isTurnButtonLongPressed.Value = false;
        }
    }

    private IEnumerator CheckTurnButtonHoldTime() {
        yield return new WaitForSeconds(0.2f);
        if (!isTurnButtonLongPressed.Value)
            OnAutoTurnInput.Raise();
    }

    // 足踏み（K+Space）長押し
    private bool isFootStep = false;
    public void OnFootStep(InputAction.CallbackContext context) {
        if (context.started) {
            isFootStep = true;
            FootStepContinuously();
        }
        if (context.canceled) {
            isFootStep = false;
        }
    }
    private async void FootStepContinuously() {
        if (!isFootStep) return;
        while (isFootStep) {
            OnFootStepInput.Raise();
            await Task.Delay(50);
        }
    }

    // ─────────────────────────────
    //  UI 系
    // ─────────────────────────────
    public void OnMenuOpen(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnMenuOpenInput.Raise();
    }
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
        Vector2 nav = context.ReadValue<Vector2>();
        if (nav.magnitude == 0) return;
        OnNavigateInput.RaiseEvent(nav);
    }
    public void OnSubmit(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        OnSubmitInput.Raise();
    }
}
