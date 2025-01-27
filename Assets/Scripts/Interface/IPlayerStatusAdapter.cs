using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerStatusAdapter
{    
    public int Level {get;set;}
    public int MaxHealth{get;set;}
    public int health {get;set;}
    public int MaxSatiety {get;set;}//最大満腹度
    public int Satiety {get;set;}//満腹度
    public int MaxMuscle {get;set;}//最大ちから
    public int Muscle{get;set;}//ちから
    public int BasicAttackPower {get;set;}//基礎攻撃力　レベルで変動
    public int DefencePower {get;set;}//防御力
    public int Experience {get;set;}//経験値
    public WeaponSO EquipWeapon{get;set;}//装備中の武器
    public ShieldSO EquipShield{get;set;}//装備中の盾
}
