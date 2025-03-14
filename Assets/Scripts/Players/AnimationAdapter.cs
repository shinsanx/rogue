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
    
    
    //SO生成用
    public void CreateSOInstance(){
        MoveAnimationDirection = ScriptableObject.CreateInstance<Vector2Variable>();
    }
 
}
