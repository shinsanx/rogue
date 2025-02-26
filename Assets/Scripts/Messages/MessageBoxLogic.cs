// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using System.Threading;
// using System.Threading.Tasks;
// using DG.Tweening;
// using UnityEditor;
// using System;

// public class MessageBoxLogic
// {

//     private readonly IMessageDisplay _messageDisplay;
//     private readonly GameObject _box;
//     private readonly GameObject _texts;


//     // private TextMeshProUGUI firstTextArea;
//     // private TextMeshProUGUI secondTextArea;
//     // private TextMeshProUGUI thirdTextArea;
//     // private TextMeshProUGUI fourthTextArea;
//     // private GameObject messageBoxObj;
//     // private MessageBox messageBox;
//     // private GameObject texts;

//     private List<List<string>> messages = new List<List<string>>();

//     public delegate void DisplayDelegate(bool visible);
//     private DisplayDelegate display;

//     //コンストラクタ
//     public MessageBoxLogic(
//         IMessageDisplay messageDisplay,
//         GameObject box,
//         GameObject texts,
//         // MessageBox messageBox,
//         // GameObject messageBoxObj,
//         // GameObject texts,
//         DisplayDelegate display
//         ){
//             this.display = display;
//             this._messageDisplay = messageDisplay;
//             this._box = box;
//             this._texts = texts;
//             // this.messageBoxObj = messageBoxObj;
//             // this.messageBox = messageBox;
//             // this.texts = texts;

//             // firstTextArea = messageBox.firstText;
//             // secondTextArea = messageBox.secondText;
//             // thirdTextArea = messageBox.thirdText;
//             // fourthTextArea = messageBox.fourthText;
//     }

//     private readonly Queue<List<string>> messageQueue = new Queue<List<string>>();
//     private bool isProcessing = false;

//     public async Task CreateMessageAsync(object data){
//         List<string> strings = (List<string>)data;
//         if(strings == null) return;

//         if(!messageBoxObj.activeSelf) {
//             display(true);
//         }
//         messageQueue.Enqueue(strings);


//         if (!isProcessing) {
//             await ProcessMessages();
//         }


        
//         messages.Add(strings);
//         await OutputMessage(strings);
//         messages.Remove(strings);
//     }

//     private async Task ProcessMessages(){
//         isProcessing = true;

//         while(messageQueue.Count > 0){
//             var strings = messageQueue.Dequeue();
//             await _messageDisplay.DisplayMessagesAsync();
//         }
//         isProcessing = false;
//         await _
//     }

//     private async Task OutputMessage(List<string> strings){
//         firstTextArea.text = null;
//         secondTextArea.text = null;
//         texts.transform.DOLocalMove(Vector3.zero, 0.01f);

//         switch(strings.Count){
//             case 1:
//                 firstTextArea.text = strings[0];
//                 break;
//             case 2:
//                 firstTextArea.text = strings[0];
//                 await Task.Delay(300);
//                 secondTextArea.text = strings[1];
//                 break;
//             case 3:
//                 break;
//             case 4:
//                 firstTextArea.text = strings[0];
//                 await Task.Delay(300);
//                 secondTextArea.text = strings[1];
//                 thirdTextArea.text = strings[2];
//                 fourthTextArea.text = strings[3];
//                 await Task.Delay(200);
//                 texts.transform.DOLocalMove(new Vector3(0, 30, 0),0.3f).SetEase(Ease.Linear);
//                 await Task.Delay(500);
//                 texts.transform.DOLocalMove(new Vector3(0, 58, 0), 0.3f).SetEase(Ease.Linear);
//                 break;
//         }
//         await Task.Delay(3000);

//         //入力コマンド数が2以上なら抜けてウィンドウを閉じないようにする
//         if(messages.Count >= 2){
//             return;
//         }
//         HideMessageBox();
//     }

//     //メッセージボックスを閉じる
//     private void HideMessageBox(){
//         Display(false);
//     }

//     public void Display(bool visible){
//         display(visible);
//     }

// }
