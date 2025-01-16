using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class EnemyMoveLogic
{
    private IPositionAdapter positionAdapter;
    private EnemyAnimLogic enemyAnimLogic;
    private Vector2 moveOffset = new Vector2(.5f, .5f);

    //コンストラクタ
    public EnemyMoveLogic(
        IPositionAdapter positionAdapter,
        EnemyAnimLogic enemyAnimLogic
    ) {
        this.positionAdapter = positionAdapter;
        this.enemyAnimLogic = enemyAnimLogic;
    }

    public void Move(Vector2Int targetPos, Vector2Int direction){
        enemyAnimLogic.SetMoveAnimation(new Vector2(direction.x, direction.y));

        Vector2 newPosition = targetPos + moveOffset;
        positionAdapter.Position = newPosition.ToVector2Int(); 
    }
}
