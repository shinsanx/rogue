using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading.Tasks;

public class UserInput : MonoBehaviour
{
    public UnityEvent<Vector2> onMoveInput;
    public UnityEvent onAttack;
    Vector2 inputVector;
    public bool isMoveButtonLongPrresed = false;
    private bool isInputLocked = true;  // 入力ロックのフラグ

    private async void Start()
    {
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
}
