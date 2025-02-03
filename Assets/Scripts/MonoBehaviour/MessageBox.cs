using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class MessageBox : MonoBehaviour, IMessageDisplay {
    public TextMeshProUGUI firstText;
    public TextMeshProUGUI secondText;
    public TextMeshProUGUI thirdText;
    public TextMeshProUGUI fourthText;
    public Vector2 initialTextsPos;

    [SerializeField] GameObject texts;
    [SerializeField] GameObject box;
    public MessageBoxLogic messageBoxLogic;

    private void Start() {
        messageBoxLogic = new MessageBoxLogic(this, box, texts, (visible) => box.SetActive(visible));
        MessageBus.Instance.Subscribe("sendMessage", messageBoxLogic.CreateMessage);
        initialTextsPos = texts.transform.position;
    }

    public async Task DisplayMessagesAsync(List<string> messages) {
        ClearAllTexts();

        TextMeshProUGUI[] textFields = {firstText, secondText, thirdText, fourthText};
        for(int i = 0; i < messages.Count && i < textFields.Length; i++){
            textFields[i].text = messages[i];

            await Task.Delay(300);
        }
        await Task.Delay(3000);

        await ShowAsync(false);
        
    }


    public async Task ShowAsync(bool visible) {
        box.SetActive(visible);
        await Task.CompletedTask;
    }

    // すべてのテキストをクリア
    private void ClearAllTexts(){
        firstText.text = string.Empty;
        secondText.text = string.Empty;
        thirdText.text = string.Empty;
        fourthText.text = string.Empty;
    }


}