using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerState", menuName = "StateMachine/PlayerState", order = 0)]
public class PlayerState : State {
    [SerializeField] GameEvent playerStateComplete;
    [SerializeField] IntVariable sleepTurnCount;
    [SerializeField] BoolVariable canHandleInput;

    //行動ゲージ関連
    [SerializeField] IntVariable playerTimeGage;
    [SerializeField] IntVariable playerActionRate;

    public override async void OnEnter() {
        canHandleInput.Value = false;
        playerTimeGage.Value += playerActionRate.Value;

        if (playerTimeGage.Value >= 100) {
            canHandleInput.Value = true;
        } else {
            playerStateComplete.Raise();
        }
        // if (sleepTurnCount.Value > 0) {
        //     sleepTurnCount.Value--;
        //     await Task.Delay(1000);
        //     playerStateComplete.Raise();
        //     Debug.Log("残りの睡眠ターン:" + sleepTurnCount.Value);
        // }
        // canHandleInput.Value = true;

    }

    public override void OnExit() {
        canHandleInput.Value = false;
        // PlayerStateが終了したときの処理を書く
    }
}
