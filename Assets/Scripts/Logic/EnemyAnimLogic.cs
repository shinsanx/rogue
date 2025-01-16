using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyAnimLogic
{
    private IAnimationAdapter animationAdapter;
    private SpriteRenderer spriteRenderer;
    private Tween tween = null;

    public EnemyAnimLogic(IAnimationAdapter animationAdapter, SpriteRenderer spriteRenderer){
        this.animationAdapter = animationAdapter;
        this.spriteRenderer = spriteRenderer;
    }

    public void SetDamageAnimation(){
        animationAdapter.TakeDamageAnimation = true;
    }

    public void DefeatedAnimation(){
        tween = spriteRenderer.DOFade(0, 2f).SetEase(Ease.Flash, 6);        
    }

    public void SetAttackAnimation(Vector2Int direction){
        animationAdapter.AttackAnimation = true;
        animationAdapter.MoveAnimationDirection = direction;
    }

    public void SetMoveAnimation(Vector2 vector){
        animationAdapter.MoveAnimationDirection = vector;
    }

    //オブジェクトを削除するときにTweenをKillする用。
    public void KillTween(){
        if(DOTween.instance != null){
            tween?.Kill();
        }
    }
}
