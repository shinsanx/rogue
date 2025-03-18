using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    // 現在アクティブなメニュー（BaseMenuControllerを継承している各メニュー）
    private BaseMenuController activeMenu;
    public List<BaseMenuController> activeMenus = new List<BaseMenuController>();
    public UnityEvent onMenuClosed;

    [SerializeField] private GameEvent OnPlayerStateComplete;
    [SerializeField] private Vector2Variable playerFaceDirection;
    [SerializeField] private CreateMessageLogic createMessageLogic;
    public ItemEventChannelSO OnItemRemoved;
    [SerializeField] private MessageEventChannelSO onMessageSend;
    



    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;        
    }

    /// <summary>
    /// 各メニューが有効になったタイミング（OnEnableなど）で呼び出して、
    /// アクティブなメニューとして登録します。
    /// </summary>
    public void RegisterMenu(BaseMenuController menu) {
        activeMenu = menu;
    }

    /// <summary>
    /// EnterキーなどによるSubmit入力時に、現在アクティブなメニューのSubmitを呼び出す。
    /// </summary>
    public void Submit() {
        if (activeMenu != null) {
            activeMenu.Submit();
        } else {
            Debug.LogWarning("アクティブなメニューが登録されていません。");
        }
    }

    /// <summary>
    /// 移動キーによる入力時に、現在アクティブなメニューのNavigateを呼び出す。  
    /// </summary>
    /// <param name="direction"></param>
    public void Navigate(Vector2Int direction) {
        if (activeMenu != null) {
            activeMenu.Navigate(direction);
        }
    }

    public void OpenMenu() {
        activeMenu.OpenMenu();
        RegisterActiveMenu(activeMenu);
        RegisterMenu(activeMenu);
        // Debug.Log(activeMenu.GetType().Name + "を開きました。");
    }

    public void CloseMenu() {
        // 現在の activeMenu を閉じる
        activeMenu.CloseMenu();

        // activeMenus から閉じたメニューを削除
        UnregisterActivesMenu(activeMenu);
        // Debug.Log(activeMenu.GetType().Name + "を閉じました。");

        // 残っているメニューがあれば、リストの末尾（最後に登録されたメニュー）を activeMenu に設定
        if (activeMenus.Count > 0) {
            activeMenu = activeMenus[activeMenus.Count - 1];
            // Debug.Log(activeMenu.GetType().Name + "が新しいアクティブメニューに設定されました。");
        } else {
            activeMenu = null;
            // Debug.Log("アクティブなメニューが存在しません。");
        }
    }

    public void CloseAllMenus() {
        int roopCount = activeMenus.Count;
        for (int i = 0; i < roopCount; i++) {
            CloseMenu();
        }
        activeMenus.Clear();
        activeMenu = null;
        onMenuClosed?.Invoke();
    }

    public void UnregisterActivesMenu(BaseMenuController menu) {
        activeMenus.Remove(menu);
        // Debug.Log(menu.GetType().Name + "を解除しました。");        
    }

    public void RegisterActiveMenu(BaseMenuController menu) {
        activeMenus.Add(menu);
        // Debug.Log(menu.GetType().Name + "を登録しました。");        
    }

    public T SetActiveMenu<T>() where T : BaseMenuController {
        activeMenu = GetComponent<T>();
        OpenMenu();
        return activeMenu as T;
    }

    //特定のメニューを閉じる
    public void CloseSpecificMenu<T>() where T : BaseMenuController {
        BaseMenuController specificMenu = GetComponent<T>();
        if (specificMenu != null) {
            specificMenu.CloseMenu();
            UnregisterActivesMenu(specificMenu);
        }
    }


    //メニューアクション
    // アイテムを使用するメソッド
    public void UseItem(ItemSO item, IEffectReceiver receiver) {
        OnItemRemoved.RaiseEvent(item);
        // アイテムの使用処理
        if (item is ConsumableSO consumable) {            
            onMessageSend.RaiseEvent(createMessageLogic.CreateUseItemMessage(consumable.itemName));
            consumable.effect.ApplyEffect(receiver);
            OnPlayerStateComplete.Raise();
        } else {
            Debug.Log("アイテムの使用に失敗しました。");
        }
    }

    //アイテムを置く
    public void PlaceItem(ItemSO item, Vector2Int position) {        
        ArrangeManager.i.PlaceItem(position, item);
        OnItemRemoved.RaiseEvent(item);
        OnPlayerStateComplete.Raise();        
        onMessageSend.RaiseEvent(createMessageLogic.CreatePlaceItemMessage(item.itemName));
    }

    //アイテムを投げる
    public async Task ThrowItem(ItemSO item, Vector2Int position) {
        Vector2Int direction = playerFaceDirection.Value.RoundVector2().ToVector2Int();
        Debug.Log("ThrowItemDirection:" + direction);
        Vector2Int throwPosition = TileManager.i.GetCharactersInFront(position, direction, 10);
        //throwPositionのタイプが壁だった場合は一つ前のポジションを取得する
        if (TileManager.i.GetMapChipType(throwPosition) == (int)RandomDungeonWithBluePrint.Constants.MapChipType.Wall) {
            throwPosition = throwPosition - direction;
        }
        //throwPositionのタイプがEnemyだった場合はアイテムの効果を適用する
        if (CharacterManager.i.GetObjectTypeByPosition(throwPosition) == "Enemy") {
            ItemEffectManager.i.ApplyItemEffect(item, CharacterManager.i.GetObjectByPosition(throwPosition));
            await AnimationManager.i.throwItemAnimation(item, position, throwPosition);
            OnItemRemoved.RaiseEvent(item);
            OnPlayerStateComplete.Raise();
            return;
        }        
        await AnimationManager.i.throwItemAnimation(item, position, throwPosition);
        ArrangeManager.i.PlaceItem(throwPosition, item);
        OnItemRemoved.RaiseEvent(item);
        OnPlayerStateComplete.Raise();
        onMessageSend.RaiseEvent(createMessageLogic.CreateThrowItemMessage(item.itemName));
    }
}