using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAdapter : MonoBehaviour, IAnimationAdapter
{
    [field:SerializeField] private Animator animator;
    [field:SerializeField] public Vector2Variable MoveAnimationDirection{get;private set;}
    
    public BoolVariable AttackAnimation{get;set;}
    public BoolVariable TakeDamageAnimation{get;set;}
    public BoolVariable EatAnimation{get;set;}
        
    public void OnMoveAnimation(){
        animator.SetFloat("x", MoveAnimationDirection.Value.x);
        animator.SetFloat("y", MoveAnimationDirection.Value.y);
    }

    public void OnAttackAnimation(){
        animator.SetTrigger("AtkTrigger");
    }

    public void OnTakeDamageAnimation(){
        animator.SetTrigger("TakeDamageTrigger");
    }

    public void OnEatAnimation(){
        animator.SetTrigger("EatTrigger");
    }
    
    
    // public BoolVariable AttackAnimation {
    //     get => animator.GetBool("AtkTrigger");
    //     set => animator.SetTrigger("AtkTrigger");
    // }

    // public Vector2Variable MoveAnimationDirection {
    //     get => playerFaceDirection.Value;
    //     set {
    //         playerFaceDirection.SetValue(value);
    //         animator.SetFloat("x", value.x);
    //         animator.SetFloat("y", value.y);
    //     }
    // }

    // public BoolVariable TakeDamageAnimation {
    //     get => animator.GetBool("TakeDamageTrigger");
    //     set => animator.SetTrigger("TakeDamageTrigger");
    // }

    // public BoolVariable EatAnimation {
    //     get => animator.GetBool("EatTrigger");
    //     set => animator.SetTrigger("EatTrigger");
    // }

    // public void SetAttackAnimation(){        
    //     MoveAnimationDirection = playerFaceDirection.Value;
    //     AttackAnimation.SetValue(true);
    // }

    // public void SetMoveAnimation(){
    //     MoveAnimationDirection = playerFaceDirection.Value;
    // }

    // public void SetTakeDamageAnimation(){
    //     TakeDamageAnimation.SetValue(true);
    // }

    // public void SetEatAnimation(){
    //     EatAnimation.SetValue(true);
    // }
}
