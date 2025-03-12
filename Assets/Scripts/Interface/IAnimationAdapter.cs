using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationAdapter
{
    public Vector2Variable MoveAnimationDirection {get;}
    public BoolVariable AttackAnimation {get;}
    public BoolVariable TakeDamageAnimation{get;}
    public BoolVariable EatAnimation {get;}
    }
