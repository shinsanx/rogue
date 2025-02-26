using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimLogic
{
    IAnimationAdapter animationAdapter;

    public PlayerAnimLogic(IAnimationAdapter animationAdapter){
        this.animationAdapter = animationAdapter;
    }

    public void SetMoveAnimation(Vector2 vector){
        animationAdapter.MoveAnimationDirection = vector;
    }

    public void SetAttackAnimation(){        
        animationAdapter.AttackAnimation = true;
    }
}
