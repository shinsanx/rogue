using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IObjectData
{
    private SpriteRenderer spriteRenderer;
    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemSO.icon;
    }
    // ========================================================
    // オブジェクトデータ
    // ========================================================
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public Vector2Int Position {
        get => _position;
        set{
            _position = value;
            transform.position = value.ToVector2() + moveOffset;
            RoomNum = TileManager.i.LookupRoomNum(value);
            OnObjectUpdated?.Invoke(this);
        }
    }
    public int RoomNum { get; set; }

    // キャラクターマネージャーによって更新される
    public event System.Action<IObjectData> OnObjectUpdated;

    
    public ItemSO itemSO;
    private Vector2Int _position;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);

    public void Initialize()
    {
        Id = CharacterManager.GetUniqueID();
        Name = itemSO.name;
        Type = "Item";
        Position = new Vector2Int(0, 0);
        RoomNum = 0;
        gameObject.GetComponent<SpriteRenderer>().sprite = itemSO.icon;
        CharacterManager.i.AddCharacter(this);
    }    

    public void OnPicked(PlayerInventory playerInventory){        
        // インベントリへの追加が成功した場合のみ、アイテムを削除
        if (playerInventory.AddItem(itemSO))
        {
            MessageBus.Instance.Publish("sendMessage", GameAssets.i.createMessageLogic.CreatePickUpMessage(itemSO.itemName));
            CharacterManager.i.RemoveCharacter(this);
            Destroy(gameObject);
        }
        else
        {
            MessageBus.Instance.Publish("sendMessage", GameAssets.i.createMessageLogic.CreateCantPickUpMessage(itemSO.itemName));
        }
    }
    

}
