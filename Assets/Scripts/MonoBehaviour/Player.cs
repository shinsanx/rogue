using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEditor;

[RequireComponent(typeof(Animator))]

public class Player : MonoBehaviour, IPositionAdapter, IAnimationAdapter, IDamageable, IPlayerStatusAdapter
{

    [SerializeField] RandomDungeonWithBluePrint.RandomMapTest randomMapTest; //アタッチ
    [SerializeField] DungeonEventManager dungeonEventManager; //アタッチ
    [SerializeField] Animator animator; //アタッチ
    [SerializeField] UserInput userInput; //on~系のイベント登録のためにアタッチ

    private PlayerMoveLogic playerMoveLogic;
    private PlayerAttackLogic playerAttackLogic;
    private TileLogic tileLogic;
    private PlayerAnimLogic playerAnimLogic;
    private PlayerStatusDataLogic playerStatusDataLogic;
    private CreateMessageLogic createMessageLogic;
    private Vector2Int playerPosition;
    private Vector2 playerFaceDirection;

    private Vector2 moveOffset = new Vector2(.5f, .5f);
    private int _currentHp;
    private int _maxHp;
    private int _currentLv;
    private int _totalExp;

    // === IPositionAdapter ===
    public Vector2Int Position {
        get {return playerPosition;}
        set{
            playerPosition = value;
            transform.DOMove(value.ToVector2() + moveOffset, 0.3f).SetEase(Ease.Linear);
        }
    }

    public int CharacterType {
        get { return (int)DungeonConstants.ObjTypelnTile.Player;}
        set{}
    }

    // === IAnimationAdapter ===
    public bool AttackAnimation {
        set{animator.SetTrigger("AttackTrigger");}
    }

    public Vector2 MoveAnimationDirection {
        get{ return playerFaceDirection;}
        set{
            playerFaceDirection = new Vector2(value.x, value.y);
            animator.SetFloat("x", value.x);
            animator.SetFloat("y", value.y);
        }
    }

    public bool TakeDamageAnimation {
        set{ animator.SetTrigger("TakeDamageTrigger");}
    }

    // === IStatusAdapter ===
    public string Name {get; set;}
    public int level {
        get{return _currentLv;}
        set {
            _currentLv = value;
            playerStatusDataLogic.LevelUp();
            if (_currentLv >= 2) MessageBus.Instance.Publish(DungeonConstants.sendMessage, createMessageLogic.LvUppedMessage(Name, _currentLv));
            MessageBus.Instance.Publish(DungeonConstants.UpdateLvText,this);
        }
    }

    public int MaxHealth {
        get {return _maxHp;}
        set {
            _maxHp = value;
            MessageBus.Instance.Publish(DungeonConstants.UpdateHPText, this);
        }
    }

    public int health {
        get {return _currentHp;}
        set {
            _currentHp = value;
            if(_currentHp > MaxHealth) _currentHp = MaxHealth;
            MessageBus.Instance.Publish(DungeonConstants.UpdateHPText, this);
        }
    }

    public int Level {get;set;}
    public int MaxSatiety {get;set;}
    public int Satiety {get;set;}
    public int MaxMuscle {get;set;}
    public int Muscle {get;set;}
    public int BasicAttackPower {get;set;}
    public int DefencePower {get;set;}
    public int Experience {
        get {return _totalExp;}
        set{
            _totalExp += value;
            if(_totalExp >= DungeonConstants.necessarryExp[level + 1]){
            level++;
            }
        }
    }        

    public WeaponSO EquipWeapon{get;set;}//装備中の武器
    public ShieldSO EquipShield{get;set;}//装備中の盾

    private void Start(){
        playerPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        tileLogic = new TileLogic();
        //マップ生成より先に取得してしまうとFieldが空になる。いったんStartにしてるけど修正しないといけない。
        playerAnimLogic = new PlayerAnimLogic(this);
        playerMoveLogic = new PlayerMoveLogic(this,playerAnimLogic);
        playerAttackLogic = new PlayerAttackLogic(playerAnimLogic,this,this,this);
        playerStatusDataLogic = new PlayerStatusDataLogic(this,this);
        createMessageLogic = new CreateMessageLogic();
        userInput.onAttack.AddListener(playerAttackLogic.Attack);
        userInput.onMoveInput.AddListener(playerMoveLogic.MoveByInput);
        randomMapTest.onFieldUpdate.AddListener(tileLogic.UpdateFieldInformation);
        playerStatusDataLogic.SetStatusDefault(this);
        MessageBus.Instance.Subscribe(DungeonConstants.GetExp, playerStatusDataLogic.GetExp);
    }

    public void TakeDamage(int damage, string dealerName){
        playerStatusDataLogic.TakeDamage(damage, dealerName);
    }
}
