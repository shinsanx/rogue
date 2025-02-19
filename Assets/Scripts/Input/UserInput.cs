using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading.Tasks;

public class UserInput : MonoBehaviour
{
    // インスペクターで設定
    public UnityEvent<Vector2> onMoveInput;
    public UnityEvent onAttack;
    public UnityEvent onMenuOpen;
    public UnityEvent onMenuClose;
    public UnityEvent<Vector2> onNavigate;

    Vector2 inputVector;
    public bool isMoveButtonLongPrresed = false;
    private bool isInputLocked = true;  // 入力ロックのフラグ

    [SerializeField]
    private InputActionAsset inputActionAsset;

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;

    private async void Start()
    {
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
    public void OnMove(InputAction.CallbackContext context){
        if (isInputLocked) return;  // ロック中は入力を無視

        if(context.started){        
            isMoveButtonLongPrresed = true;
            return;
        }
        if(context.canceled){
            isMoveButtonLongPrresed = false;
            return;
        }
        inputVector = context.ReadValue<Vector2>();
        onMoveInput.Invoke(inputVector);
    }

    //長押し移動
    public void OnLongPress(InputAction.CallbackContext context){
        if (isInputLocked) return;  // ロック中は入力を無視

        if(context.performed){
            MoveContinuously();
        }
    }

    private async void MoveContinuously(){
        if(!isMoveButtonLongPrresed) return;
        while(isMoveButtonLongPrresed){
            onMoveInput.Invoke(inputVector);
            await Task.Delay(300);
        }
    }

    public void OnAttack(InputAction.CallbackContext context){
        if (isInputLocked) return;  // ロック中は入力を無視

        if(context.started) return;
        if(context.canceled) return;
        onAttack?.Invoke();
    }

    /// <summary>
    /// メニューを開く際に呼び出す
    /// </summary>
    public void OnMenuOpen(InputAction.CallbackContext context)
    {
        if(context.started) return;
        if(context.canceled) return;
        Debug.Log("メニューを開く");
        playerActionMap.Disable();
        uiActionMap.Enable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onMenuOpen?.Invoke();
    }

    /// <summary>
    /// メニューを閉じる際に呼び出す
    /// </summary>
    public void OnMenuClose(InputAction.CallbackContext context)
    {
        if(context.started) return;
        if(context.canceled) return;
        Debug.Log("メニューを閉じる");
        uiActionMap.Disable();
        playerActionMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onMenuClose?.Invoke();
    }

    public void OnNavigate(InputAction.CallbackContext context){
        if(context.started) return;
        if(context.canceled) return;
        Vector2 navigateVector = context.ReadValue<Vector2>();
        Debug.Log("navigateVector: " + navigateVector);
        onNavigate?.Invoke(navigateVector);
    }
}
