using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading.Tasks;

public class UserInput : MonoBehaviour {
    // インスペクターで設定                
    Vector2 inputVector;
    public bool isMoveButtonLongPrresed = false;
    private bool isInputLocked = true;  // 入力ロックのフラグ

    public GameEvent OnAttackInput;
    public Vector2EventChannelSO OnMoveInput;
    public GameEvent OnMenuOpenInput;
    public GameEvent OnMenuCloseInput;
    public Vector2EventChannelSO OnNavigateInput;
    public GameEvent OnSubmitInput;

    [SerializeField]
    private InputActionAsset inputActionAsset;

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    private async void Start() {
        // ActionMapの取得
        playerActionMap = inputActionAsset.FindActionMap("Player", true);
        uiActionMap = inputActionAsset.FindActionMap("UI", true);

        // 初期状態ではPlayer ActionMapを有効化し、UI ActionMapを無効化
        playerActionMap.Enable();
        uiActionMap.Disable();

        // 2秒間入力をロック
        isInputLocked = true;
        await Task.Delay(3000);
        isInputLocked = false;
    }

    //移動
    public void OnMove(InputAction.CallbackContext context) {
        if (isInputLocked) return;  // ロック中は入力を無視

        if (context.started) {
            isMoveButtonLongPrresed = true;
            return;
        }
        if (context.canceled) {
            isMoveButtonLongPrresed = false;
            return;
        }
        inputVector = context.ReadValue<Vector2>();        
        OnMoveInput.RaiseEvent(inputVector);
    }

    //長押し移動
    public void OnLongPress(InputAction.CallbackContext context) {
        if (isInputLocked) return;  // ロック中は入力を無視

        if (context.performed) {
            MoveContinuously();
        }
    }

    private async void MoveContinuously() {
        if (!isMoveButtonLongPrresed) return;
        while (isMoveButtonLongPrresed) {            
            OnMoveInput.RaiseEvent(inputVector);
            await Task.Delay(300);
        }
    }

    public void OnAttack(InputAction.CallbackContext context) {
        if (isInputLocked) return;  // ロック中は入力を無視

        if (context.started) return;
        if (context.canceled) return;
        //onAttack?.Invoke();
        OnAttackInput.Raise();
    }

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

    public void OnToggleActionMap(){
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

    public void OnEnableActionMap(){
        playerActionMap.Enable();
        uiActionMap.Disable();
    }

    public void OnDisableActionMap(){
        playerActionMap.Disable();
        uiActionMap.Enable();
    }

    public void OnNavigate(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        Vector2 navigateVector = context.ReadValue<Vector2>();
        if (navigateVector.magnitude == 0) return;
        //onNavigate?.Invoke(navigateVector);
        OnNavigateInput.RaiseEvent(navigateVector);
    }

    public void OnSubmit(InputAction.CallbackContext context) {
        if (context.started) return;
        if (context.canceled) return;
        //onSubmit?.Invoke();
        OnSubmitInput.Raise();
    }
}
