using System.Collections.Generic;
using UnityEngine;

public class MessagePresenter : MonoBehaviour {
    [SerializeField] MessageView view;

    MessageModel model;

    void Awake() {
        model = new MessageModel(this);
        model.OnChanged += () => view.Render(model.Entries);
    }

    /* -------- 変更：List<string> 受け取り -------- */
    public void Send(List<string> msgs) => model.PushMany(msgs);
    /* -------------------------------------------- */
}