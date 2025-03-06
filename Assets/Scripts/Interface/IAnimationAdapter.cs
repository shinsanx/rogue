using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationAdapter
{
    public Vector2 MoveAnimationDirection {get; set;}
    public bool AttackAnimation {set;}
    public bool TakeDamageAnimation{set;}
    public bool EatAnimation {set;}
    }
