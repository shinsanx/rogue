using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "StateMachine/PlayerState", order = 0)]
public class PlayerState : State {
    [SerializeField] GameEvent playerStateComplete;    

    //入力関連
    [SerializeField] BoolVariable canHandleInput;
    [SerializeField] BoolVariable canPlayerMove;

    //行動ゲージ関連
    [SerializeField] IntVariable playerTimeGage;
    [SerializeField] IntVariable playerActionRate;

    //アイテム関連
    [SerializeField] GameEvent playerTickStatusEffects;

    public override async void OnEnter() {
        canHandleInput.Value = true;
        canPlayerMove.Value = true;

        // StatusEffectのTick実行
        playerTickStatusEffects.Raise();

        if (!canHandleInput.Value) {
            Debug.Log("行動できない状態");
            await Task.Delay(500);
            playerStateComplete.Raise();
            return;
        }

        // 行動ゲージ処理
        playerTimeGage.Value += playerActionRate.Value;

        if (playerTimeGage.Value >= 100) {
            canHandleInput.Value = true;
        } else {
            playerStateComplete.Raise();
        }
    }

    public override void OnExit() {
        canHandleInput.Value = false;
        canPlayerMove.Value = false;
        // PlayerStateが終了したときの処理を書く
    }
}
