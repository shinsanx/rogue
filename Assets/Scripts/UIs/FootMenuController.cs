using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FootMenuController : BaseMenuController
{
    [SerializeField] GameObject footMenuWindow;
    [SerializeField] CurrentSelectedObjectSO currentSelectedObjectSO;
    [SerializeField] GameObject slotItemPrefab;
    [SerializeField] Transform slotItemParent;

    public override void OpenMenu() {        
        footMenuWindow.SetActive(true);
        ClearSlot();
        DisplayItem();
        isActive = true;
    }

    public override void CloseMenu() {
        footMenuWindow.SetActive(false);        
        isActive = false;
    }

    public override void Submit() {
        SubMenuController subMenuController = MenuManager.Instance.SetActiveMenu<SubMenuController>(); 
        currentSelectedObjectSO.SubmitMenuSet = currentSelectedObjectSO.Object.GetComponent<IMenuActionAdapter>().submitMenuSet;
    }

    private void DisplayItem() {
        GameObject slot = Instantiate(slotItemPrefab, slotItemParent);
        SetItemInformation(slot);
        menuItems.Add(slot);
    }

    private void SetItemInformation(GameObject slot) {
        Image icon = slot.transform.Find("Icon").GetComponent<Image>();
        string type = currentSelectedObjectSO.Object.GetComponent<ObjectData>().Type.Value;
        if (type == "Item") {
            icon.sprite = currentSelectedObjectSO.Object.GetComponent<Item>().itemSO.icon;
        } else {                
            Debug.Log("アイテムにアイコンが設定されていません。");
        }

        TextMeshProUGUI itemName = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
        itemName.text = currentSelectedObjectSO.Object.GetComponent<ObjectData>().Name.Value;
    }

    private void ClearSlot() {
        foreach (Transform child in slotItemParent) {
            Destroy(child.gameObject);
        }
        menuItems.Clear();
    }

    public void OpenStairMenu() {
        currentSelectedObjectSO.SubmitMenuSet = currentSelectedObjectSO.Object.GetComponent<IMenuActionAdapter>().submitMenuSet;
        MenuManager.Instance.SetActiveMenu<FootMenuController>();
        Submit();
    }

    public void OpenItemMenu() {
        currentSelectedObjectSO.SubmitMenuSet = currentSelectedObjectSO.Object.GetComponent<IMenuActionAdapter>().submitMenuSet;
        MenuManager.Instance.SetActiveMenu<FootMenuController>();
        Submit();
    }
}
