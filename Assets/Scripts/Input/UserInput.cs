using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

public class UserInput : MonoBehaviour {
    // インスペクターで設定                
    Vector2 inputVector;
    [SerializeField] private BoolVariable isMoveButtonLongPrresed;
    [SerializeField] private BoolVariable isTurnButtonLongPressed;
    
    // 斜め入力バッファリング用
    [SerializeField] private float diagonalInputBufferTime = 0.1f; // バッファ時間（秒）
    private Vector2 lastInputDirection = Vector2.zero;
    private float lastInputTime = 0f;
    private bool isBufferingInput = false;
    private Coroutine bufferCoroutine = null;
    
    // 入力ロック機構
    [SerializeField] private float inputLockDuration = 0.2f; // 入力ロック時間（秒）
    private bool isInputLocked = false;
    private Coroutine inputLockCoroutine = null;

    // イベント
    public GameEvent OnAttackInput;
    public Vector2EventChannelSO OnMoveInput;
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

    //移動
    public void OnMove(InputAction.CallbackContext context) {        
        if (context.started) {
            isMoveButtonLongPrresed.Value = true;
            return;
        }
        if (context.canceled) {
            isMoveButtonLongPrresed.Value = false;
            return;
        }
        
        // 入力がロックされている場合は処理しない
        if (isInputLocked) {
            return;
        }
        
        // 現在の入力を取得
        Vector2 currentInput = context.ReadValue<Vector2>();
        
        // 入力値を四捨五入して方向ベクトルに変換
        Vector2 roundedInput = new Vector2(
            Mathf.Round(currentInput.x),
            Mathf.Round(currentInput.y)
        );        
        
        // 入力がない場合は処理しない
        if (roundedInput == Vector2.zero) return;
        
        // 現在時刻を取得
        float currentTime = Time.time;
        
        // 前回の入力からの経過時間を計算
        float timeSinceLastInput = currentTime - lastInputTime;
        
        // バッファ時間内に新しい入力があった場合
        if (timeSinceLastInput <= diagonalInputBufferTime && lastInputDirection != Vector2.zero) {
            // 前回の入力と現在の入力を組み合わせて斜め入力を作成
            Vector2 combinedInput = lastInputDirection + roundedInput;
            
            // 斜め入力の正規化（-1〜1の範囲に収める）
            combinedInput.x = Mathf.Clamp(combinedInput.x, -1f, 1f);
            combinedInput.y = Mathf.Clamp(combinedInput.y, -1f, 1f);
            
            // 斜め入力になっている場合のみ処理
            if (Mathf.Abs(combinedInput.x) > 0 && Mathf.Abs(combinedInput.y) > 0) {
                // 実行中のコルーチンがあれば停止
                if (bufferCoroutine != null) {
                    StopCoroutine(bufferCoroutine);
                    bufferCoroutine = null;
                }
                
                inputVector = combinedInput;
                OnMoveInput.RaiseEvent(inputVector);
                
                // バッファをリセット
                lastInputDirection = Vector2.zero;
                isBufferingInput = false;
                
                // 入力をロックして余分な入力を防止
                LockInput();
                
                return;
            }
        }
        
        // バッファリング中でない場合は通常の入力処理
        if (!isBufferingInput) {
            // バッファリングを開始
            isBufferingInput = true;
            lastInputDirection = roundedInput;
            lastInputTime = currentTime;
            
            // 実行中のコルーチンがあれば停止
            if (bufferCoroutine != null) {
                StopCoroutine(bufferCoroutine);
            }
            
            // バッファ時間後に入力を処理するコルーチンを開始
            bufferCoroutine = StartCoroutine(ProcessBufferedInput());
        } else {
            // バッファリング中に新しい入力があった場合は更新
            lastInputDirection = roundedInput;
            lastInputTime = currentTime;
        }
    }
    
    // バッファ時間後に入力を処理するコルーチン
    private IEnumerator ProcessBufferedInput() {
        yield return new WaitForSeconds(diagonalInputBufferTime);
        
        // バッファ時間後も斜め入力が検出されなかった場合は、最後の入力を処理
        if (isBufferingInput && lastInputDirection != Vector2.zero) {
            inputVector = lastInputDirection;
            OnMoveInput.RaiseEvent(inputVector);
            
            // バッファをリセット
            lastInputDirection = Vector2.zero;
            isBufferingInput = false;
        }
        
        bufferCoroutine = null;
    }
    
    // 入力をロックする
    private void LockInput() {
        isInputLocked = true;
        
        // 既存のロック解除コルーチンがあれば停止
        if (inputLockCoroutine != null) {
            StopCoroutine(inputLockCoroutine);
        }
        
        // 指定時間後に入力ロックを解除するコルーチンを開始
        inputLockCoroutine = StartCoroutine(UnlockInputAfterDelay());
    }
    
    // 指定時間後に入力ロックを解除するコルーチン
    private IEnumerator UnlockInputAfterDelay() {
        yield return new WaitForSeconds(inputLockDuration);
        isInputLocked = false;
        inputLockCoroutine = null;
    }




    //長押し移動
    public void OnLongPress(InputAction.CallbackContext context) {        

        if (context.performed) {
            MoveContinuously();
        }
    }

    // 長押し移動
    private async void MoveContinuously() {
        if (!isMoveButtonLongPrresed.Value) return;
        while (isMoveButtonLongPrresed.Value) {
            OnMoveInput.RaiseEvent(inputVector);
            await Task.Delay((int)(moveSpeed.Value * 1000));
        }
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
        if(context.started) {
            isFootStep = true;
            FootStepContinuously();
        }
        if(context.canceled) {
            isFootStep = false;
        }
    }

    // 足踏み長押し処理
    private async void FootStepContinuously() {
        if(!isFootStep) return;
        while(isFootStep) {
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
