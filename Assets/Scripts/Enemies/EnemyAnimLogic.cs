using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class EnemyAnimLogic {
    private AnimationAdapter animationAdapter;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Tween tween = null;

    public EnemyAnimLogic(AnimationAdapter animationAdapter, SpriteRenderer spriteRenderer) {
        this.animationAdapter = animationAdapter;
        this.spriteRenderer = spriteRenderer;
        this.animator = animationAdapter.GetComponent<Animator>();
    }

    public void SetDamageAnimation() {
        animationAdapter.OnTakeDamageAnimation();
    }

    public void DefeatedAnimation() {
        tween = spriteRenderer.DOFade(0, 2f).SetEase(Ease.Flash, 6);
    }

    public void SetAttackAnimation(
    Vector2Int direction,
    float approachDistance = 0f,
    System.Action OnComplete = null) {

        var moveDir = new Vector3(direction.x, direction.y, 0).normalized;
        var originPos = spriteRenderer.transform.localPosition;
        var targetPos = originPos + moveDir * approachDistance;

        Sequence seq = DOTween.Sequence();
        //seq.AppendInterval(0.2f);

        // ② 攻撃アニメ（OnMoveAnimation, OnAttackAnimationはそのまま）
        seq.InsertCallback(0f,() => {
            animationAdapter.MoveAnimationDirection.SetValue(direction);
            animationAdapter.OnMoveAnimation();
            animationAdapter.OnAttackAnimation();
            Debug.Log("anim");
        });

        if (approachDistance > 0f) {
            // ① 近づく
            seq.Append(spriteRenderer.transform.DOLocalMove(targetPos, 0.07f));
        }

        

        // ③ 元に戻る
        if (approachDistance > 0f) {
            seq.Append(spriteRenderer.transform.DOLocalMove(originPos, 0.09f));
        }

        // // ④ 完了コールバック
        // if (OnComplete != null) {
        //     seq.OnComplete(() => OnComplete());
        // }

        if (OnComplete != null) {
            animationAdapter.StartCoroutine(WaitAttackEnd(OnComplete));
        }
    }

    /* 攻撃ステートが終わるのを待つ */
    private IEnumerator WaitAttackEnd(System.Action cb) {
        // ① Attack ステートに入るのを待つ
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));


        // ② Attack が終わる（タグが外れる）まで待つ
        yield return new WaitUntil(() =>
            !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));

        cb.Invoke();
    }



    public void SetMoveAnimation(Vector2 vector) {
        animationAdapter.MoveAnimationDirection.SetValue(vector);
        animationAdapter.OnMoveAnimation();
    }

    //オブジェクトを削除するときにTweenをKillする用。
    public void KillTween() {
        if (DOTween.instance != null) {
            tween?.Kill();
        }
    }
}
