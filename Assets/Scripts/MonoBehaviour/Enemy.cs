using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(Animator))]

public class Enemy : MonoBehaviour, IPositionAdapter, IDamageable, IMonsterStatusAdapter, IAnimationAdapter
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] MonsterStatusSO slimeSO;
    [SerializeField] MessageBox messageBox;
    private RandomDungeonWithBluePrint.RandomMapTest randomMapTest; //アタッチ
    private EnemyStatusLogic enemyStatusLogic;
    private EnemyAnimLogic enemyAnimLogic;
    private EnemyAttackLogic enemyAttackLogic;
    private EnemyAILogic enemyAILogic;

    Vector2 moveOffset = new Vector2(.5f, .5f);
    private Vector2Int _enemyPosition; //startでtransform.position入れてるけど危険
    private int _hp;


    //=== IPositionAdapter ===
    public Vector2Int Position{
        get{return _enemyPosition;}
        set{transform.DOMove(value.ToVector2() + moveOffset, (0.3f)).SetEase(Ease.Linear);
        _enemyPosition = value;
        //Debug.Log(_enemyPosition + "enemyPos");
        }
    }

    public int CharacterType {
        get {
            return (int)DungeonConstants.ObjTypelnTile.Enemy;
        }
        set{}
    }

    // === IAnimationAdapter ===
    private Vector2 enemyFaceDirection;
    public Vector2 MoveAnimationDirection {
        get{ return enemyFaceDirection;}
        set {
            enemyFaceDirection = new Vector2(value.x, value.y);
            animator.SetFloat("x", value.x);
            animator.SetFloat("y", value.y);
        }
    }

    public bool AttackAnimation {
        set {
            animator.SetTrigger("AtkTrigger");
        }
    }

    public bool TakeDamageAnimation {
        set {
            animator.SetTrigger("TakeDamageTrigger");
        }
    }

    string IMonsterStatusAdapter.Name {get;set;}
    int IMonsterStatusAdapter.HP {
        get {return _hp;}
        set {
            if(_hp <= value) {} //TODO: 回復アニメーションを再生する＆ダメージアニメーションを再生しない
            _hp = value;
            enemyAnimLogic.SetDamageAnimation();
            if(_hp <= 0){
                enemyAnimLogic.SetDamageAnimation();
                enemyStatusLogic.Destroy();
            }
        }
    }

    public int AttackPower {get;set;}
    public int Defence {get;set;}
    public int Exp {get;set;}
    public string MoveSpeed {get;set;}
    public string Attribute {get;set;}
    public float ItemDropRate {get;set;}
    public Sprite Sprite {get{return null;} set{sr.sprite = value;}}
    public RuntimeAnimatorController AnimatorController{
        get{return animator.runtimeAnimatorController;}
        set{animator.runtimeAnimatorController = value;}
    }

    public bool Action {
        get {return true;}
        set{enemyAILogic.AIStart();}
    }

    private void Start(){
        _enemyPosition = transform.position.ToVector2Int();
        enemyAnimLogic = new EnemyAnimLogic(this, sr);
        enemyStatusLogic = new EnemyStatusLogic(this, slimeSO);
        enemyStatusLogic.OnDestroyed += () => Destroy(gameObject);
        enemyAILogic = new EnemyAILogic(this, this, this, sr);

        //MoveAnimationDirection = new Vector2(0,-1); //初期の方向　仮で一旦下を向くように
    }

    public void TakeDamage(int damage, string dealerName){
        enemyStatusLogic.TakeDamage(damage, dealerName);
    }

    private void OnDisable() {
        enemyAnimLogic.KillTween();
    }

    
}
