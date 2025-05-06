using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageView : MonoBehaviour {
    [Header("UI Refs")]
    [SerializeField] TextMeshProUGUI[] lines;  // Line0 = 最上段, Line2 = 最下段
    [SerializeField] RectTransform root;

    [Header("Animation")]
    [SerializeField] float slideTime = .2f;  // スライド所要
    [SerializeField] float fadeTime = .25f;  // フェード所要
    [SerializeField] float spacing = 4f;    // LayoutGroup.Spacing と合わせる

    Vector2 basePos;                           // root 初期座標


    readonly Queue<string> _current = new();   // 直近の表示内容 (最古→最新)

    /* ------------------------------- */

    void Awake() => basePos = root.anchoredPosition;

    /* ====== Presenter から呼ばれる ====== */
    public void Render(IEnumerable<string> shown) {
        var list = new List<string>(shown);     // list[0] 最古, list[^1] 最新
        
        bool hasAdd =
        list.Count > _current.Count
        || (list.Count == _current.Count
            && list.Count > 0
            && list[^1] != _current.ToArray()[^1]);        

        if (hasAdd)                            // 新規行が増えた！
            PlaySlideAnimation(list);
        else
            ApplyListToLines(list);            // 行数変化なし/減少

        /* ★ 追加：行数が減ったときのために必ず同期 */
        if (!hasAdd) SyncCurrent(list);
    }

    /* ---- スライド演出 ---- */
    void PlaySlideAnimation(List<string> finalList) {
                
        float lineH = lines[0].rectTransform.sizeDelta.y + spacing;        
        if (Mathf.Approximately(lineH, 0f)) lineH = 30f;    // 念のためフォールバックï                     

        /* ① 今はまだ旧表示のまま */
        ApplyListToLines(new List<string>(_current));

        /* ② root を上へアニメーション */
        root.DOComplete();
        root.DOAnchorPos(basePos + new Vector2(0, lineH), slideTime)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                root.anchoredPosition = basePos;

                /* ③ アニメ完了後に最終並びへ更新 */
                ApplyListToLines(finalList);
            });

        SyncCurrent(finalList);
    }

    /* ---- 行⇆UI 反映共通 ---- */
    void ApplyListToLines(List<string> list) {
        int offset = lines.Length - list.Count;    // 空き行 (上側)
        for (int i = 0; i < lines.Length; i++) {
            int src = i - offset;
            if (src >= 0) {
                lines[i].text = list[src];
                lines[i].gameObject.SetActive(true);
                lines[i].alpha = 1f;
            } else {
                lines[i].gameObject.SetActive(false);
            }
        }
    }

    /* ---- _current を最新化 ---- */
    void SyncCurrent(List<string> list) {
        _current.Clear();
        foreach (var s in list) _current.Enqueue(s);
    }

    /* ==== 1 行 or 全行フェード用の API (オプション) ==== */
    public void FadeOutTop() {
        if (lines[0].gameObject.activeSelf)
            lines[0].DOFade(0, 0.15f).OnComplete(() => lines[0].gameObject.SetActive(false));
    }

    public void FadeOutAll() {
        foreach (var t in lines)
            if (t.gameObject.activeSelf)
                t.DOFade(0, fadeTime).OnComplete(() => t.gameObject.SetActive(false));
    }
}