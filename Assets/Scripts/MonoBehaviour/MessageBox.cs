using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageBox : MonoBehaviour
{
    public TextMeshProUGUI firstText;
    public TextMeshProUGUI secondText;
    public TextMeshProUGUI thirdText;
    public TextMeshProUGUI fourthText;
    public Vector2 initialTextsPos;

    [SerializeField] GameObject texts;
    [SerializeField] GameObject box;
    public MessageBoxLogic messageBoxLogic;

    private void Start(){
        messageBoxLogic = new MessageBoxLogic(this, box, texts,(visible) => box.SetActive(visible));
        MessageBus.Instance.Subscribe("sendMessage", messageBoxLogic.CreateMessage);
        initialTextsPos = texts.transform.position;
    }
}
