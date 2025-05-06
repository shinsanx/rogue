using UnityEngine;

public class MessagePresenter : MonoBehaviour {
    [SerializeField] MessageView view;

    MessageModel model;

    void Awake() {
        model = new MessageModel(this);
        model.OnChanged += () => view.Render(model.Shown);
        model.OnTimeout += view.FadeOutAll;
        model.OnOverFlow += view.FadeOutTop;
    }

    /* まとめ送信口 */
    public void Send(System.Collections.Generic.List<string> msgs) => model.PushMany(msgs);
}