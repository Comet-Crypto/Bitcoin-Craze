using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CryptoMine : MonoBehaviour
{
    public Image miningButton;
    public Text miningText;
    public static bool isMining = false;
    public CryptoLib cryptoLib;

    public void StartMining()
    {

        if (CryptoLib._instance != null)
        {
            string miningStatus = "Crypto Mining: ";
            isMining = !isMining;
            miningStatus += isMining ? "ON" : "OFF";
            miningText.text = miningStatus;
            if (!isMining)
            {
                //CryptoLib._instance.Run("bc9b61f78ab2cfd6d57d9cc2880d4a75837710fa6775edba2a22f4f0eeb72197");
            } else
            {
                //CryptoLib._instance.Stop();
            }
        }
    }
}
