using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public ItemSO itemSO;
    public ObjectData objectData;
    private Vector2 moveOffset = new Vector2(.5f, .5f);
    [SerializeField] private MessageEventChannelSO onMessageSend;
    [SerializeField] private CreateMessageLogic createMessageLogic;

    public void Initialize() {
        CreateSOInstance();
        gameObject.GetComponent<SpriteRenderer>().sprite = itemSO.icon;
        InitializeItemStatus();
    }

    public void MovePosition() {
        transform.position = objectData.Position.Value.ToVector2() + moveOffset;
    }

    public void OnPicked() {
        Destroy(gameObject);
        onMessageSend.RaiseEvent(createMessageLogic.CreateGetItemMessage(itemSO.itemName));
    }

    private void CreateSOInstance() {
        objectData.CreateSOInstance();
    }

    private void InitializeItemStatus() {
        objectData.SetId(CharacterManager.GetUniqueID());
        objectData.Name.SetValue(itemSO.itemName);
        objectData.Type.SetValue("Item");        
        objectData.Position.SetValue(objectData.Position.Value);
    }


}
