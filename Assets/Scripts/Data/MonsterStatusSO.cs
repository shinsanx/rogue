using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatusSO", menuName = "Status", order = 0)]
public class MonsterStatusSO : ScriptableObject {

    public string Name;
    public int HP;
    public int AttackPower;
    public int Deffence;
    public int Exp;
    public int MoveSpeed;
    public string Attribute;
    public float ItemDropRate;
    public Sprite Sprite;
    public RuntimeAnimatorController AnimatorController;
}
