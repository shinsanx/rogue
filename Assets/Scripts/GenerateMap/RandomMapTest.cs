using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RandomDungeonWithBluePrint {
    public class RandomMapTest : MonoBehaviour {

        [Serializable]
        public class BluePrintWithWeight {
            public FieldBluePrint BluePrint = default;
            public int Weight = default;//BluePringを複数設定した場合の重さ
        }

        [SerializeField] private int seed = default;
        [SerializeField] private Button generateButton = default;
        [SerializeField] private FieldView fieldView = default;
        [SerializeField] private BluePrintWithWeight[] bluePrints = default;
        public UnityEvent<Field> onFieldUpdate;//フィールドを生成したときにPlayerにfieldを渡すときのEvent.Playerクラスからメソッドを渡す
        public Field currentField; //現在生成されているField

        private void Awake() { //マップ生成が発火するところ
            Random.InitState(seed);
            generateButton.onClick.AddListener(() => Create(Raffle()));
            generateButton.onClick.Invoke();
        }

        private void Create(BluePrintWithWeight bluePrint) {
            var field = FieldBuilder.Build(bluePrint.BluePrint);
            currentField = field;
            //MessageBus.Instance.DelegateSubscribe<Field>(DungeonConstants.GetCurrentField, GetCurrentField);
            //MessageBus.Instance.Publish("UpdateFieldInformation", currentField);
            fieldView.ShowField(field);
            onFieldUpdate?.Invoke(field);
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
    }
}