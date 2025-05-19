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
    public string MoveSpeed;
    public string Attribute;
    public float ItemDropRate;
    public Sprite Sprite;
    public RuntimeAnimatorController AnimatorController;
    public ApproachType ApproachType = ApproachType.None;
    public float CustomApproachDistance = 0f;
}


public enum ApproachType
{
    None,     // 近づかない
    Short,    // 少し近づく
    Long,     // 大きく近づく
    Custom    // ScriptableObjectで個別に距離指定
}