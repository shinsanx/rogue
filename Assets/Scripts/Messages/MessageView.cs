using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MessageView : MonoBehaviour {
    [SerializeField] TextMeshProUGUI[] lines; // size = 3
    [SerializeField] RectTransform root;      // ViewRoot
    [SerializeField] float slideTime = 0.25f; // スライド時間

    readonly Queue<string> _current = new();

    void Awake() {
        // LayoutGroup がある場合、手動で anchoredPosition を触るので
        // childForceExpand を切っとくと挙動が安定
        if (root.TryGetComponent(out VerticalLayoutGroup vlg))
            vlg.childForceExpandHeight = false;
    }

    public void Render(IEnumerable<string> entries) {
        // 1) entries → List にコピー（最新が list[0]）
        var list = new List<string>(entries);
        //list.Reverse();

        // 2) 追加行数を判定
        int added = list.Count - _current.Count;
        if (added > 0) {
            // 既存 root を上方向に slide
            float lineHeight = lines[0].rectTransform.sizeDelta.y + 4f; // 4 = spacing
            root.DOComplete(); // 旧アニメ終端
            root.DOAnchorPosY(root.anchoredPosition.y + lineHeight * added, slideTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => {
                    // アニメ完了後に位置リセットして見た目を保つ
                    root.anchoredPosition = Vector2.zero;
                    RefreshTexts(list);
                });

            // アニメ中も文字を更新しておく（新規行は一旦非表示）
            RefreshTexts(list, added);
        } else {
            // 行が減った or 変化なし → そのまま更新
            RefreshTexts(list);
        }

        // _current を同期
        _current.Clear();
        foreach (var s in list) _current.Enqueue(s);
    }

    /// <summary>
    /// texts を list で更新。filter == added の時は最新行をまだ表示しない
    /// </summary>
    void RefreshTexts(List<string> list, int hideTopN = 0) {
        for (int i = 0; i < lines.Length; i++) {
            if (i < list.Count) {
                lines[i].text = list[i];
                lines[i].gameObject.SetActive(i >= hideTopN); // スライド前は非表示
            } else {
                lines[i].gameObject.SetActive(false);
            }
        }
    }
}