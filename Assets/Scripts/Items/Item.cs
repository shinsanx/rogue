using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemSO.icon;
    }
    // ========================================================
    // オブジェクトデータ
    // ========================================================
    public IntVariable Id{get;set;}
    public StringVariable Name{get;set;}
    public StringVariable Type{get;set;}
    public IntVariable RoomNum{get;set;}
    [SerializeField] private Vector2IntVariable _position;

    public Vector2IntVariable Position {
        get => _position;
        set{
            _position.SetValue(value);
            transform.position = Position.Value.ToVector2() + moveOffset;
            RoomNum.SetValue(TileManager.i.LookupRoomNum(Position.Value));
        }
    }

    public void SetPosition(Vector2Int position) {
        Position.SetValue(position);
        transform.position = Position.Value.ToVector2() + moveOffset;
        RoomNum.SetValue(TileManager.i.LookupRoomNum(Position.Value));
    }
    // キャラクターマネージャーによって更新される
    public event System.Action<IObjectData> OnObjectUpdated;

    
    public ItemSO itemSO;
    private Vector2 moveOffset = new Vector2(0.5f, 0.5f);

    public void Initialize()
    {
        Id.SetValue(CharacterManager.GetUniqueID());
        Name.Value=itemSO.name;
        Type.Value="Item";
        Position.SetValue(new Vector2Int(0, 0));
        RoomNum.SetValue(0);
        gameObject.GetComponent<SpriteRenderer>().sprite = itemSO.icon;        
    }    

    public void OnPicked(PlayerInventory playerInventory){        
        // インベントリへの追加が成功した場合のみ、アイテムを削除
        if (playerInventory.AddItem(itemSO))
        {
            MessageBus.Instance.Publish("sendMessage", GameAssets.i.createMessageLogic.CreatePickUpMessage(itemSO.itemName));            
            Destroy(gameObject);
        }
        else
        {
            MessageBus.Instance.Publish("sendMessage", GameAssets.i.createMessageLogic.CreateCantPickUpMessage(itemSO.itemName));
        }
    }
    

}
