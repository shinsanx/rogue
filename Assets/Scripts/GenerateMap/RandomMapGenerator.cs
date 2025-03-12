using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RandomDungeonWithBluePrint {
    public class RandomMapGenerator : MonoBehaviour {

        [Serializable]
        public class BluePrintWithWeight {
            public FieldBluePrint BluePrint = default;
            public int Weight = default;//BluePringを複数設定した場合の重さ
        }
        
        [SerializeField] private Button generateButton = default;
        [SerializeField] private FieldView fieldView = default;
        [SerializeField] private BluePrintWithWeight[] bluePrints = default;        
        public Field currentField; //現在生成されているField

        private void Create(BluePrintWithWeight bluePrint) {
            var field = FieldBuilder.Build(bluePrint.BluePrint);
            currentField = field;
            MessageBus.Instance.Subscribe<Field>(DungeonConstants.GetCurrentField, GetCurrentField);            
            fieldView.ShowField(field);            
        }

        private BluePrintWithWeight Raffle() {
            var candidate = bluePrints.ToList();
            var rand = Random.Range(0, candidate.Sum(c => c.Weight));
            var pick = 0;
            for (var i = 0; i < candidate.Count; i++) {
                if (rand < candidate[i].Weight) {
                    pick = i;
                    break;
                }
                rand -= candidate[i].Weight;
            }
            return candidate[pick];
        }

        //自作
        public Field GetCurrentField(object data) {
            return currentField;
        }

        public void CreateMiniMap(){
            MessageBus.Instance.Publish("UpdateMiniMap", currentField);
        }        

        public void Initialize() {
            Random.InitState(DateTime.Now.Millisecond);            
            generateButton.onClick.AddListener(() => Create(Raffle()));
            generateButton.onClick.Invoke();
        }
    }
}