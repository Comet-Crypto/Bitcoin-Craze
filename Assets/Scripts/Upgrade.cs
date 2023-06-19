using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Upgrade : MonoBehaviour
{
    public TextMeshProUGUI tDetails;
    public string strMainDetailsText;
    public string strSubDetailsText;
    public TextMeshProUGUI tCost;
    private BigInteger nCost;
    public TextMeshProUGUI tLevel;
    private int nLevel;
    public int nMaxLevel;
    public Button btnBuy;
    public bool isClickPowerUpgrade;

    private void Start()
    {
        tDetails.text = strMainDetailsText + "\n" + strSubDetailsText;
        tCost.text = "Cost\n0";
        nLevel = 0;
        tLevel.text = "Lv: " + nLevel + " / " + nMaxLevel;
    }

    public void LevelUp(BigInteger newCost, string newCostText)
    {
        nLevel++;
        tLevel.text = "Lv: " + nLevel + " / " + nMaxLevel;
        nCost = newCost;
        tCost.text = "Cost\n" + newCostText;
        if(nLevel == nMaxLevel)
        {
            tCost.text = "Max\nLevel";
        }
    }

    // Getter for nCost
    public BigInteger GetCost()
    {
        return nCost;
    }

    // Setter for nCost
    public void SetCost(BigInteger newCost)
    {
        nCost = newCost;
    }

    // Getter for nLevel
    public int GetLevel()
    {
        return nLevel;
    }

    // Setter for nLevel
    public void SetLevel(int newLevel)
    {
        nLevel = newLevel;
    }
}