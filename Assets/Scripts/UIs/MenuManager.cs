using UnityEngine;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    // 現在アクティブなメニュー（BaseMenuControllerを継承している各メニュー）
    private BaseMenuController activeMenu;

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

    public void ToggleMenu() {
        if (activeMenu.isActive) {
            activeMenu.CloseMenu();
        } else {
            activeMenu.OpenMenu();
        }

    }

    public T SetActiveMenu<T>() where T : BaseMenuController {
        activeMenu = GetComponent<T>();
        return activeMenu as T;
    }
}