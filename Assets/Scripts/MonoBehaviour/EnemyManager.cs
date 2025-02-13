using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    
    private List<Enemy> enemies = new List<Enemy>();
    private StateMachine stateMachine;
    private State enemyState;

    public void Initialize() {
        stateMachine = GameAssets.i.stateMachine;
        enemyState = GameAssets.i.enemyState;
    }

    public async Task ProcessEnemies(){

        if(stateMachine.CurrentState != enemyState) return;            
    }

    private async Task ProcessEnemyAction(Enemy enemy) {
        
    }
}
