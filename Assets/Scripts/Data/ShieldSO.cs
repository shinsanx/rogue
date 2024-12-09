using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ShieldDataSO", menuName = "ShieldData", order = 0)]
public class ShieldSO : ScriptableObject
{
    public string shieldName;
    [TextArea] public string description;
    public int power;
    public int purchacePrice;
    public int sellingPrice;
    public int enhancementValue; //強化値
}
