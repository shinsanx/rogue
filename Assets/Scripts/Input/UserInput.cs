using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading.Tasks;

public class UserInput : MonoBehaviour {
    // インスペクターで設定                
    Vector2 inputVector;
    [SerializeField] private BoolVariable isMoveButtonLongPrresed;
    [SerializeField] private BoolVariable isTurnButtonLongPressed;

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
            // Debug.Log("playerActionMap.Disable");
        } else if (MenuManager.Instance.activeMenus.Count == 0) {
            playerActionMap.Enable();
            uiActionMap.Disable();
            // Debug.Log("playerActionMap.Enable");
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
        inputVector = context.ReadValue<Vector2>();
        OnMoveInput.RaiseEvent(inputVector);
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

    // 足踏み
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



