using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IMenuActionAdapter {
    private SpriteRenderer spriteRenderer;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public BaseItemSO itemSO;
    public ObjectData objectData;
    private Vector2 moveOffset = new Vector2(.5f, .5f);
    [SerializeField] private MessageEventChannelSO onMessageSend;
    [SerializeField] private CreateMessageLogic createMessageLogic;
    [SerializeField] private GameEvent onFootItem;
    [SerializeField] private CurrentSelectedObjectSO currentSelectedObjectSO;
    [field:SerializeField] public SubmitMenuSet submitMenuSet{get;private set;}

    public void Initialize() {
        CreateSOInstance();
        gameObject.GetComponent<SpriteRenderer>().sprite = itemSO.icon;
        InitializeItemStatus();
    }

    public void MovePosition() {
        transform.position = objectData.Position.Value.ToVector2() + moveOffset;
    }

    public void OnPicked() {
        onMessageSend.RaiseEvent(createMessageLogic.CreateGetItemMessage(itemSO.itemName));
        Destroy(gameObject);
    }

    private void CreateSOInstance() {
        objectData.CreateSOInstance();
    }

    private void InitializeItemStatus() {
        objectData.SetId(CharacterManager.GetUniqueID());
        objectData.Name.SetValue(itemSO.itemName);
        objectData.Type.SetValue("Item");        
        objectData.Position.SetValue(transform.position);
    }

    public void OnSelected() {
        currentSelectedObjectSO.Object = gameObject;
        onFootItem.Raise();     
    }

    public void OnGetOnItem() {
        onMessageSend.RaiseEvent(createMessageLogic.CreateGetOnItemMessage(itemSO.itemName));
    }


}
