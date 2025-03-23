using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubMenuPrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI menuText;

    public void SetMenuText(string text) {
        menuText.text = text;
    }
}
