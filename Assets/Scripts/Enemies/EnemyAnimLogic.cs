using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyAnimLogic
{
    private AnimationAdapter animationAdapter;
    private SpriteRenderer spriteRenderer;
    private Tween tween = null;

    public EnemyAnimLogic(AnimationAdapter animationAdapter, SpriteRenderer spriteRenderer){
        this.animationAdapter = animationAdapter;
        this.spriteRenderer = spriteRenderer;
    }

    public void SetDamageAnimation(){
        animationAdapter.OnTakeDamageAnimation();
    }

    public void DefeatedAnimation(){
        tween = spriteRenderer.DOFade(0, 2f).SetEase(Ease.Flash, 6);        
    }

    public void SetAttackAnimation(Vector2Int direction){          
        animationAdapter.MoveAnimationDirection.SetValue(direction);
        animationAdapter.OnMoveAnimation();
        animationAdapter.OnAttackAnimation();
    }

    public void SetMoveAnimation(Vector2 vector){        
        animationAdapter.MoveAnimationDirection.SetValue(vector);
        animationAdapter.OnMoveAnimation();
    }

    //オブジェクトを削除するときにTweenをKillする用。
    public void KillTween(){
        if(DOTween.instance != null){
            tween?.Kill();
        }
    }
}
