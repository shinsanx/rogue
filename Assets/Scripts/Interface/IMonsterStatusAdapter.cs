using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonsterStatusAdapter
{    
    public int HP {get; set;}
    public int AttackPower {get;set;}
    public int Defence {get; set;}
    public int Exp {get; set;}
    public string MoveSpeed{get; set;}
    public string Attribute {get; set;}
    public float ItemDropRate {get; set;}
    public Sprite Sprite{get;set;}
    public RuntimeAnimatorController AnimatorController{get;set;}
    public bool Action {set;}
}
