using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTableSO", menuName = "Dungeon/EnemyTableSO")]
public class EnemyTableSO : ScriptableObject
{
    public List<MonsterStatusSO> Enemies;
}
