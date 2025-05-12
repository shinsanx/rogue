using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerAttackLogic {
    private IObjectData objectData;
    private DamageCalculate damageCalculate;
    private bool isAttacking = false;
    private Player player;
    private ObjectDataRuntimeSet objectDataSet;
    private Vector2Variable playerFaceDirection;

    //イベント
    private GameEvent OnPlayerStateComplete;
    private GameEvent OnPlayerAttack;
    private GameEvent OnPlayerDirectionChanged;


    //コンストラクタ
    public PlayerAttackLogic(
        Player player,
        GameEvent OnPlayerStateComplete,
        ObjectDataRuntimeSet objectDataSet,
        Vector2Variable playerFaceDirection,
        GameEvent OnPlayerAttack,
        GameEvent OnPlayerDirectionChanged
    ) {
        this.player = player;
        this.objectData = player.playerObjectData;
        this.OnPlayerStateComplete = OnPlayerStateComplete;
        this.objectDataSet = objectDataSet;
        this.playerFaceDirection = playerFaceDirection;
        this.OnPlayerAttack = OnPlayerAttack;
        this.OnPlayerDirectionChanged = OnPlayerDirectionChanged;
    }

    public void Attack() => AttackAsync();

    public async Task AttackAsync() {
        if (isAttacking) return;
        isAttacking = true;
        OnPlayerAttack.Raise();

        DealDamage(objectDataSet.GetObjectByPosition(GetTargetPosition()));

        await Task.Delay(500);

        OnPlayerStateComplete.Raise();
        isAttacking = false;
    }

    private Vector2Int GetTargetPosition() {
        int selfXpos = Mathf.FloorToInt(objectData.Position.Value.x);
        int selfYpos = Mathf.FloorToInt(objectData.Position.Value.y);
        int faceXpos = (int)Mathf.Round(playerFaceDirection.Value.x);
        int faceYpos = (int)Mathf.Round(playerFaceDirection.Value.y);
        Vector2Int targetVector = new Vector2Int(selfXpos + faceXpos, selfYpos + faceYpos);
        return targetVector;
    }

    private void DealDamage(GameObject targetObject) {
        if (targetObject == null) return;
        if (!targetObject.CompareTag("Enemy")) return;
        if (damageCalculate == null) {
            damageCalculate = new DamageCalculate();
        }

        int weaponPw;
        if (player.EquipWeapon != null) {
            weaponPw = player.EquipWeapon.attackPower;
        } else {
            weaponPw = 1;
        }

        IMonsterStatusAdapter monsterStatusAdapter = targetObject.GetComponent<IMonsterStatusAdapter>();
        int damage = damageCalculate.CalculateAttackDamage(player.playerLevel.Value, player.playerMaxMuscle.Value, weaponPw, monsterStatusAdapter.Defence);
        IDamageable damageable = targetObject.GetComponent<IDamageable>();
        damageable.TakeDamage(damage, objectData.Name.Value);
    }


    public bool ConfusionAttack() {
        _ = ConfusionAttackAsync();
        return true;
    }

    public async Task<bool> ConfusionAttackAsync() {
        if (isAttacking) return false;
        isAttacking = true;
        OnPlayerAttack.Raise();

        List<Vector2Int> surroundingPositions = TileManager.i.GetSurroundingPositions(objectData.Position.Value);
        int randomIndex = UnityEngine.Random.Range(0, surroundingPositions.Count);
        Vector2Int randomPosition = surroundingPositions[randomIndex];

        DealDamage(objectDataSet.GetObjectByPosition(randomPosition));
        await Task.Delay(500);
        playerFaceDirection.Value = new Vector2(randomPosition.x - objectData.Position.Value.x, randomPosition.y - objectData.Position.Value.y);
        OnPlayerDirectionChanged.Raise();
        OnPlayerStateComplete.Raise();

        isAttacking = false;
        return true;
    }

}
