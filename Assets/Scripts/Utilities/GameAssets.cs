using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameAssets: MonoBehaviour {

    private static GameAssets _i;
    public static GameAssets i {
        get {
            if(_i == null) _i = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _i;
        }
    }

    public StateMachine stateMachine;
    public State playerState;
    public State enemyState;        
    public CreateMessageLogic createMessageLogic = new CreateMessageLogic();  

    public AllItemListSO allItemListSO;
    

}