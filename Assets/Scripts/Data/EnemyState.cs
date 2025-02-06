using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyState", menuName = "EnemyState/EnemyState", order = 0)]
public class EnemyState : State
{
    public override void OnEnter() {
        base.OnEnter();
        // Debug.Log("EnemyState start");
        // EnemyStateがスタートしたときの処理を書く
    }

    public override void OnExit() {
        base.OnExit();
        // EnemyStateが終了したときの処理を書く
    }
}
