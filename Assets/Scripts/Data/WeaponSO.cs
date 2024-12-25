using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDataSO", menuName ="WeaponDataSO",order =0)]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    [TextArea] public string description;
    public int power;
    public int purchasePrice;
    public int sellingPrice;
    public int enhancementValue;
}
