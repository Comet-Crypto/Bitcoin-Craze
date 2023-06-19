using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    // Clicker
    public Text coinScore;
    public BigInteger currentScore;
    public BigInteger clickPower;
    public BigInteger gainedCoinPerSecond;
    public Button coinButton;
    public List<GameObject> upgradesList;
    public AudioSource mainCameraAudioSource;
    public AudioClip upgradeAudioClip;
    private float nextSecond;
    private bool isInfinite;
    private readonly int BOOSTED_MAX = 30;
    private AudioSource audioSource;
    public Button speaker;
    public Image muteImage;

    // Start is called before the first frame update
    void Start()
    {
        currentScore = BigInteger.Parse("1"); // 3111199900000000000000000000000000000000000000000000000000000000000000000000000
        clickPower = BigInteger.Parse("1");
        gainedCoinPerSecond = BigInteger.Parse("0");
        coinButton.onClick.AddListener(ClickCoin);
        isInfinite = false;
        audioSource = GetComponent<AudioSource>();
        speaker.onClick.AddListener(() => SpeakerPressed());
        muteImage.enabled = false;

        for (int i = 0; i < upgradesList.Count; i++)
        {
            Upgrade upgrade = upgradesList[i].GetComponent<Upgrade>();
            int index = i; // Create a local variable with the current value of i

            upgradesList[i].GetComponentInChildren<Button>()
                .onClick.AddListener(() => HandleUpgradeClick(upgradesList[index]));
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (coinScore != null) {
            coinScore.text = CalcNumberToText(currentScore) + "\nBTC";
        }

        // Increment the current score with the gained coin per second once per second
        if (!isInfinite && Time.time >= nextSecond)
        {
            currentScore += gainedCoinPerSecond;
            nextSecond = Mathf.FloorToInt(Time.time) + 1;
            if (CryptoMine.isMining)
            {
                int exp = (int)Math.Floor(BigInteger.Log10(currentScore) / 3);
                currentScore += (1001 + (10 * BigInteger.Pow(1000, exp)));
            }
        }
    }

    public void ClickCoin()
    {
        if (!isInfinite)
        {
            currentScore = BigInteger.Add(currentScore, clickPower);
        }
    }

    public string CalcNumberToText(BigInteger number){
        string suffix = "";
        string strNumber = number.ToString();
        int exp;

        if (strNumber.Length > 3)
        {
            exp = (int)Math.Floor(Math.Log10(double.Parse(strNumber)) / 3);
            if(exp > 26)
            {
                isInfinite = true;
                return "Infinity";
            }
            suffix = "abcdefghijklmnopqrstuvwxyz"[exp - 1].ToString();
            strNumber = (double.Parse(strNumber) / Math.Pow(1000, exp)).ToString();
        }

        if (strNumber.Length > 3)
        {
            string[] parts = strNumber.Split(".");
            string wholePart = parts[0];
            string decimalPart = parts[1] + "0";
            decimalPart = "" + decimalPart[0] + decimalPart[1];
            strNumber = wholePart + "." + decimalPart;
            strNumber = float.Parse(strNumber).ToString("F2").TrimEnd('0');
            if(strNumber.Length == wholePart.Length + 1) strNumber = wholePart;
            
        }

        return strNumber + suffix;
    }

    // Function to calculate upgrade cost based on level and upgrade type
    BigInteger CalculateUpgradeCost(int level, bool isClickPowerUpgrade,bool isBoostedUpgrade)
    {
        BigInteger baseCost;

        if (isClickPowerUpgrade && isBoostedUpgrade)
        {
            // Click Power Upgrade Costs
            if (level >= 1 && level <= 9)
            {
                baseCost = BigInteger.Parse("30");
                return baseCost * BigInteger.Pow(2, level * 3);
            }
            else if (level >= 10 && level <= 24)
            {
                baseCost = BigInteger.Parse("80");
                return baseCost * BigInteger.Pow(3, level * 3);
            }
            else if (level >= 25 && level <= BOOSTED_MAX)
            {
                baseCost = BigInteger.Parse("300");
                return baseCost * BigInteger.Pow(4, (level + 12) * 3);
            }
        }
        else if (isClickPowerUpgrade)
        {
            // Click Power Upgrade Costs
            if (level >= 1 && level <= 9)
            {
                baseCost = BigInteger.Parse("10");
                return baseCost * BigInteger.Pow(2, level);
            }
            else if (level >= 10 && level <= 24)
            {
                baseCost = BigInteger.Parse("40");
                return baseCost * BigInteger.Pow(3, level);
            }
            else if (level >= 25 && level <= 49)
            {
                baseCost = BigInteger.Parse("90");
                return baseCost * BigInteger.Pow(4, level);
            }
            else if (level >= 50 && level <= 89)
            {
                baseCost = BigInteger.Parse("450");
                return baseCost * BigInteger.Pow(5, level);
            }
            else if (level >= 90 && level <= 100)
            {
                baseCost = BigInteger.Parse("1000");
                return baseCost * BigInteger.Pow(4, level + 21);
            }
        }
        else
        {
            // Passive Income Upgrade Costs
            if (level >= 1 && level <= 9)
            {
                baseCost = BigInteger.Parse("20");
                return baseCost * BigInteger.Pow(2, level);
            }
            else if (level >= 10 && level <= 24)
            {
                baseCost = BigInteger.Parse("100");
                return baseCost * BigInteger.Pow(3, level);
            }
            else if (level >= 25 && level <= 49)
            {
                baseCost = BigInteger.Parse("200");
                return baseCost * BigInteger.Pow(4, level);
            }
            else if (level >= 50 && level <= 89)
            {
                baseCost = BigInteger.Parse("1000");
                return baseCost * BigInteger.Pow(5, level);
            }
            else if (level >= 90 && level <= 100)
            {
                baseCost = BigInteger.Parse("1000");
                return baseCost * BigInteger.Pow(4, level + 21);
            }
        }

        return BigInteger.Zero; // Default cost if level is out of range
    }


    private void HandleUpgradeClick(GameObject gameUpgrade)
    {
        Upgrade upgrade = gameUpgrade.GetComponent<Upgrade>();

        if (upgrade.GetLevel() < upgrade.nMaxLevel)
        {
            if (upgrade.GetCost() <= currentScore)
            {
                if (!isInfinite)
                {
                    currentScore -= upgrade.GetCost();
                    audioSource.clip = upgradeAudioClip;
                    audioSource.Play();
                  
                }
                bool isBoosted = (upgrade.nMaxLevel == BOOSTED_MAX);
                BigInteger nextCost = CalculateUpgradeCost(upgrade.GetLevel() + 1, upgrade.isClickPowerUpgrade, isBoosted);
                if (upgrade.isClickPowerUpgrade)
                {
                    // Calculate the new click power based on the cost increase
                    clickPower += CalculateClickPower(nextCost);
                }
                else
                {
                    // Calculate the new gained coin per second based on the cost increase
                    gainedCoinPerSecond += CalculateGainedCoinPerSecond(nextCost);
                }
                upgrade.LevelUp(nextCost, CalcNumberToText(nextCost));
            }
        }
    }

    // Function to calculate the click power based on the upgrade level and cost
    private BigInteger CalculateClickPower(BigInteger cost)
    {
        // Modify this logic based on how you want the click power to increase
        // For example, you can divide the cost by a certain value to determine the click power
        return cost / BigInteger.Parse("10");
    }


    // Function to calculate the gained coin per second based on the upgrade level and cost
    private BigInteger CalculateGainedCoinPerSecond(BigInteger cost)
    {
        // Modify this logic based on how you want the gained coin per second to increase
        // For example, you can divide the cost by a certain value to determine the gained coin per second
        return cost / BigInteger.Parse("13");
    }

    public void SpeakerPressed()
    {
        muteImage.enabled = !muteImage.enabled;
        mainCameraAudioSource.enabled = !mainCameraAudioSource.enabled;
    }
}
