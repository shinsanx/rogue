using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomDungeonWithBluePrint;
using JetBrains.Annotations;

public class AutoMapping : MonoBehaviour {
    public GameObject roads;
    public GameObject enemies;
    public GameObject items;
    public GameObject roadImagePrefab;
    public GameObject enemyImagePrefab;
    public GameObject playerImagePrefab;
    // public GameObject itemImagePrefab;
    private Field currentField;

    float scale;
    Vector2 startPosition;
    
    private RectTransform roadsRect;

    // private void Start() {
    //     Initialize();
    // }

    private void UpdateMap(object data) {
        if (data is Field field) {
            currentField = field;
            CreateMap();
        }
    }

    public void CreateMap() {
        if (currentField == null) return;
        ClearMap();

        Vector2Int mapSize = currentField.Size;

        float scaleX = roadsRect.rect.width / mapSize.x;
        float scaleY = roadsRect.rect.height / mapSize.y;
        scale = Mathf.Min(scaleX, scaleY);

        // マップ全体のサイズを計算
        float totalWidth = mapSize.x * scale;
        float totalHeight = mapSize.y * scale;

        // 中心からのオフセットを計算
        startPosition = new Vector2(
            -totalWidth * 0.5f,
            -totalHeight * 0.5f
        );

        // 道を描画
        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                int tileType = currentField.Grid[x, y];

                if (tileType == (int)Constants.MapChipType.Floor ||
                    tileType >= (int)Constants.MapChipType.Up) {
                    CreateUIElement(roadImagePrefab, roads.transform, new Vector2Int(x, y), scale, startPosition);
                }
            }
        }

        CreateCharacterUI();
    }
    
    // 敵とプレイヤーを描画
    public void CreateCharacterUI() {
        foreach (var objectData in CharacterManager.i.allObjectData) {
            switch (objectData.Type) {
                case "Enemy":
                    CreateUIElement(enemyImagePrefab, enemies.transform, objectData.Position, scale, startPosition);
                    break;
                case "Player":
                    CreateUIElement(playerImagePrefab, enemies.transform, objectData.Position, scale, startPosition);
                    break;
            }
        }
    }

    // キャラクターのUIを描画
    private void CreateUIElement(GameObject prefab, Transform parent, Vector2Int position, float scale, Vector2 startPosition) {
        GameObject element = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        element.transform.SetParent(parent);

        RectTransform rectTransform = element.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Vector2 pos = startPosition + new Vector2(
            position.x * scale + scale * 0.5f,
            position.y * scale + scale * 0.5f
        );

        rectTransform.anchoredPosition = pos;
        rectTransform.sizeDelta = new Vector2(scale, scale);
        rectTransform.localScale = Vector3.one;
    }

    private void ClearMap() {
        // 既存のマップオブジェクトを削除
        foreach (Transform child in roads.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in enemies.transform) {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy() {
        MessageBus.Instance.Unsubscribe("UpdateFieldInformation", UpdateMap);
    }

    public void Initialize() {
        MessageBus.Instance.Subscribe("UpdateFieldInformation", UpdateMap);
        roadsRect = roads.GetComponent<RectTransform>();
    }
}
