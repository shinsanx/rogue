using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyState", menuName = "StateMachine/EnemyState", order = 0)]
public class EnemyState : State {    
    [SerializeField] GameEvent onEnemyActionStart;
    [SerializeField] GameEvent updateMiniMap;        
    
    public override void OnEnter() {
        EnemyStateStart();
        
        
    }

    public override void OnExit() {
        updateMiniMap.Raise();
    }

    public void EnemyStateStart() {
        onEnemyActionStart.Raise();        
        
    }
}
