using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculate
{
    //プレイヤーの攻撃ダメージ
    public int CalculateAttackDamage(int level, int muscle, int weaponPower, int enemyDefence){
        float rand = Random.Range(0.875f, 1.125f);
        int attackPower = CalculateAttackPower(level, muscle, weaponPower);
        int defence = (enemyDefence / 2)+1;

        float damage = Mathf.Round((attackPower - defence) * rand);
        return (int)damage;
    }

    //プレイヤー攻撃力の計算
    private int CalculateAttackPower(int level, int muscle, int weaponPower){
        int levelAttackPower = CalcurateLevelAttackPower(level);
        int weaponAttackPower = CalculateWeaponAttackPower(weaponPower, muscle);
        int attackPower = muscle + levelAttackPower + weaponAttackPower;
        
        return attackPower;
    }

    //プレイヤーレベル攻撃力の計算
    private int CalcurateLevelAttackPower(int level){
        float damageFloat = default;
        if(1 <= level && level <= 5){
            damageFloat = 1 + (level -1) * 1.5f;
            return (int)damageFloat;
        } else if(6 <= level && level <= 13){
            damageFloat = 7.5f + (level-5)*1;
            return (int)damageFloat;
        } else {
            damageFloat = 15.5f + (level -13) * 0.5f;
            return (int)damageFloat;
        }
    }

    //プレイヤー武器攻撃力の計算
    private int CalculateWeaponAttackPower(int weaponPower, int muscle){
        float weaponAttackPower = weaponPower * (0.75f * muscle / 32);
        return (int)weaponAttackPower;
    }

    //敵の攻撃ダメージ（攻撃力*乱数-守備力+1）
    public int CalculateEnemyAttackDamage(int AtkPower, int targetDfc){
        float rand = Random.Range(0.875f, 1.125f);
        float damage = Mathf.Round((AtkPower - targetDfc +1)*rand);
        return (int)damage;
    }
}
