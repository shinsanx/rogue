using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerStatusAdapter
{    

    public IntVariable playerLevel{get;}
    public IntVariable playerMaxHealth{get;}
    public IntVariable playerCurrentHealth{get;}
    public IntVariable playerMaxMuscle{get;}
    public IntVariable playerCurrentMuscle{get;}
    public IntVariable playerBasicAttackPower{get;}
    public IntVariable playerDefencePower{get;}
    public IntVariable playerExperience{get;}
    public WeaponSO EquipWeapon{get;}//装備中の武器
    public ShieldSO EquipShield{get;}//装備中の盾
}
