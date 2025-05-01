
using UnityEngine;
using RandomDungeonWithBluePrint;


public class AutoMapping : MonoBehaviour {
    public GameObject roads;
    public GameObject enemies;
    public GameObject items;
    public GameObject roadImagePrefab;
    public GameObject roomImagePrefab;
    public GameObject enemyImagePrefab;
    public GameObject playerImagePrefab;
    public GameObject itemImagePrefab;
    private Field currentField;
    [SerializeField] private ObjectDataRuntimeSet objectDataSet;
    [SerializeField] private TileManager tileManager;

    float scale;
    Vector2 startPosition;
    
    private RectTransform roadsRect;
    private Vector2 offset = new Vector2(0.5f, 0f);


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
                //int tileType = currentField.Grid[x, y];
                
                int tileType = tileManager.GetTileType(new Vector2Int(x, y));
                if(tileType == (int)Constants.TileType.Aisle) {
                    CreateUIElement(roadImagePrefab, roads.transform, new Vector2Int(x, y), scale, startPosition);
                }

                if (tileType == (int)Constants.TileType.Room) {
                    CreateUIElement(roomImagePrefab, roads.transform, new Vector2Int(x, y), scale, startPosition);
                }
            }
        }

        CreateCharacterUI();
    }
    
    // 敵とプレイヤーを描画
    public void CreateCharacterUI() {
        if (currentField == null) return;
        ClearCharacterUI();
        
        foreach (var objectData in objectDataSet.GetAllObjectData()) {
            switch (objectData.Type.Value) {
                case "Enemy":
                    CreateUIElement(enemyImagePrefab, enemies.transform, objectData.Position.Value + offset, scale, startPosition);
                    break;
                case "Player":
                    CreateUIElement(playerImagePrefab, enemies.transform, objectData.Position.Value + offset, scale, startPosition);
                    break;
                case "Item":
                    CreateUIElement(itemImagePrefab, items.transform, objectData.Position.Value + offset, scale, startPosition);
                    break;
            }
        }
    }

    private void ClearCharacterUI() {
        foreach (Transform child in enemies.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in items.transform) {
            Destroy(child.gameObject);
        }
    }

    // キャラクターのUIを描画
    private void CreateUIElement(GameObject prefab, Transform parent, Vector2 position, float scale, Vector2 startPosition) {
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
        foreach (Transform child in items.transform) {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy() {
        MessageBus.Instance.Unsubscribe("UpdateMiniMap", UpdateMap);
    }

    public void Initialize() {
        MessageBus.Instance.Subscribe("UpdateMiniMap", UpdateMap);
        MessageBus.Instance.Subscribe("CreateCharacterUI", (object data) => CreateCharacterUI());
        roadsRect = roads.GetComponent<RectTransform>();
    }
}
