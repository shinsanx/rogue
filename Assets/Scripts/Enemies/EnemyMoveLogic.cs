using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class EnemyMoveLogic
{
    private IObjectData objectData;
    private EnemyAnimLogic enemyAnimLogic;
    private Vector2 moveOffset = new Vector2(.5f, .5f);

    //コンストラクタ
    public EnemyMoveLogic(
        IObjectData objectData,
        EnemyAnimLogic enemyAnimLogic
    ) {
        this.objectData = objectData;
        this.enemyAnimLogic = enemyAnimLogic;
    }

    public void Move(Vector2Int targetPos, Vector2Int direction){
        enemyAnimLogic.SetMoveAnimation(new Vector2(direction.x, direction.y));

        Vector2 newPosition = targetPos + moveOffset;        
        objectData.SetPosition(newPosition.ToVector2Int()); 
    }
}
