using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuHandle : MonoBehaviour
{
    public Image upgradeButton;
    public Text upgradeText;
    public bool isMenuOpen = false;
    public Button coinButton;

    public void OpenMenu()
    {
        isMenuOpen = !isMenuOpen;
        upgradeText.text = isMenuOpen ? "CLOSE" : "UPGRADE";
        coinButton.interactable = !isMenuOpen;
    }
}
