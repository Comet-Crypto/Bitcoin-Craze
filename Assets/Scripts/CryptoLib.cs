using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

public class CryptoLib : MonoBehaviour
{
    // Instance
    public static CryptoLib _instance;
    public static CryptoLib Instance => _instance;

    private const string HASH_ALGORITHM = "SHA-256";
    private const int HASH_LENGTH = 64;
    private const long RANGE_SIZE = 250000;

    private Block block;
    private string apiKey;
    private bool isMining;
    private Coroutine miningCoroutine;
    private string hashAnswer;

    private float startTime; // Updated to be defined outside the functions

    // Constructor
    private void Awake()
    {
        _instance = this;
        isMining = false;
    }

    public async void Run(string apiKey)
    {
        this.apiKey = apiKey;
        isMining = true;
        miningCoroutine = StartCoroutine(MiningCoroutine());
        await Task.Yield(); // Yield control to the calling context
    }

    public void Stop()
    {
        isMining = false;
        if (miningCoroutine != null)
        {
            try
            {
                StopCoroutine(miningCoroutine);
                miningCoroutine = null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error stopping mining coroutine: " + e.Message);
            }
        }
    }

    private System.Collections.IEnumerator MiningCoroutine()
    {
        while (isMining)
        {
            yield return StartCoroutine(GetNewTask());
            yield return StartCoroutine(SendTaskResult(Mine()));
            yield return SendMineTime();
        }
    }

    // HTTP Requests
    private System.Collections.IEnumerator GetNewTask()
    {
        string url = "https://load-balancer-server.vercel.app/communication/newTask";
        WWWForm form = new WWWForm();
        form.AddField("key", apiKey);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        var asyncOperation = www.SendWebRequest();

        yield return new UnityEngine.WaitUntil(() => asyncOperation.isDone);

        try
        {
            if (www.result != UnityWebRequest.Result.Success)
            {
                // Handle error
                Debug.LogError("Error: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log("JSON Data: " + json);

            Dictionary<string, object> jsonObject = ParseJson(json);

            if (jsonObject.ContainsKey("version"))
            {
                Debug.Log("Version: " + jsonObject["version"]);
            }

            if (jsonObject.ContainsKey("bits"))
            {
                Debug.Log("Bits: " + jsonObject["bits"]);
            }

            // Print other key-value pairs as needed

            Debug.Log("jsonObject is null: " + (jsonObject == null));
            Debug.Log("jsonObject is empty: " + (jsonObject.Count == 0));
            Debug.Log("Number of key-value pairs in jsonObject: " + jsonObject.Count);
            hashAnswer = jsonObject["hash"].ToString(); // Demo mode only

            // version + previousBlockHash + merkleRoot + timestamp + bits + nonce
            block = new Block(
                Convert.ToInt32(jsonObject["version"]),
                jsonObject["previousBlockHash"].ToString(),
                jsonObject["merkleRoot"].ToString(),
                Convert.ToInt64(jsonObject["timestamp"]),
                jsonObject["bits"].ToString(),
                Convert.ToInt64(jsonObject["range"]),
                Convert.ToDouble(jsonObject["difficulty"])
            );
            Debug.Log(block.ToString());

            startTime = Time.time; // Set the startTime
        }
        catch (Exception e)
        {
            Debug.LogError("Exception: " + e.Message);
        }
    }

    private System.Collections.IEnumerator SendTaskResult(string result)
    {
        string url = "https://load-balancer-server.vercel.app/communication/taskFinished";
        Dictionary<string, string> data = new Dictionary<string, string>
        {
            { "hash", result }
        };
        string json = JsonUtility.ToJson(data);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();

        var asyncOperation = www.SendWebRequest();

        yield return new UnityEngine.WaitUntil(() => asyncOperation.isDone);

        if (www.result != UnityWebRequest.Result.Success)
        {
            // Handle error
            Debug.LogError("Error: " + www.error);
        }
    }

    [System.Serializable]
    private class JobData
    {
        public string appKey;
        public float workDuration;
    }

    /*private System.Collections.IEnumerator SendMineTime()
    {
        float elapsedTime = Time.time - startTime; // Calculate the elapsedTime

        string url = "https://web-server-chi.vercel.app/statistics/updateStatistics";

        // Create the job data object
        var jobData = new JobData
        {
            appKey = apiKey,
            workDuration = elapsedTime
        };

        var dataContainer = new
        {
            job = jobData
        };

        // Serialize the job data object into JSON
        string json = JsonUtility.ToJson(dataContainer);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = www.SendWebRequest();

        yield return new UnityEngine.WaitUntil(() => asyncOperation.isDone);

        try
        {
            if (www.result != UnityWebRequest.Result.Success)
            {
                // Handle error
                Debug.LogError("Error: " + www.error);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception while sending mine time: " + e.Message);
        }
    }*/
    private async Task SendMineTime()
    {
        float elapsedTime = Time.time - startTime; // Calculate the elapsedTime

        string url = "https://web-server-chi.vercel.app/statistics/updateStatistics";

        // Create the job data object
        var jobData = new JobData
        {
            appKey = apiKey,
            workDuration = elapsedTime
        };

        // Create the container object
        var dataContainer = new
        {
            job = jobData
        };

        // Serialize the data container object into JSON
        string json = JsonUtility.ToJson(dataContainer);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (var httpClient = new HttpClient())
        {
            using (var content = new ByteArrayContent(postData))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                try
                {
                    var response = await httpClient.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    Debug.Log("Request successful");
                }
                catch (HttpRequestException e)
                {
                    Debug.LogError("Exception while sending mine time: " + e.Message);
                }
            }
        }
    }


    // Mining Functionality

    private class Block
    {
        public int version;
        public string previousBlockHash;
        public string merkleRoot;
        public long timestamp;
        public string bits;
        public long range;
        public double difficulty;

        public Block(int version, string previousBlockHash, string merkleRoot, long timestamp,
                     string bits, long range, double difficulty)
        {
            this.version = version;
            this.previousBlockHash = previousBlockHash;
            this.merkleRoot = merkleRoot;
            this.timestamp = timestamp;
            this.bits = bits;
            this.range = range;
            this.difficulty = difficulty;
        }

        public override string ToString()
        {
            return $"Version: {version}\nPrevious Block Hash: {previousBlockHash}\nMerkle Root: {merkleRoot}\n" +
                $"Timestamp: {timestamp}\nBits: {bits}\nRange: {range}\nDifficulty: {difficulty}";
        }
    }

    private string Mine()
    {
        // Try different nonces until a valid one is found
        long nonce = block.range - RANGE_SIZE - 1;
        string hash;
        do
        {
            nonce++;
            // version + previousBlockHash + merkleRoot + timestamp + bits + nonce
            hash = CalculateHash(block.version, block.previousBlockHash, block.merkleRoot,
                block.timestamp, block.bits, nonce);
        } while (!HashMeetsDifficultyTarget(hash) && nonce < block.range);

        return hash;
    }

    private string CalculateHash(int version, string previousBlockHash, string merkleRoot, long timestamp,
                                 string bits, long nonce)
    {
        string data = version.ToString() + previousBlockHash + merkleRoot +
            timestamp.ToString() + bits + nonce.ToString();

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha256.ComputeHash(dataBytes);
            StringBuilder stringBuilder = new StringBuilder(HASH_LENGTH);
            foreach (byte b in hashBytes)
                stringBuilder.Append(b.ToString("x2"));
            return stringBuilder.ToString();
        }
    }

    private bool HashMeetsDifficultyTarget(string hash)
    {
        return hashAnswer.Equals(hash);
    }

    private Dictionary<string, object> ParseJson(string json)
    {
        Dictionary<string, object> jsonObject = new Dictionary<string, object>();

        // Remove curly braces at the beginning and end of the JSON string
        json = json.TrimStart('{').TrimEnd('}');

        // Split the string by commas to separate key-value pairs
        string[] pairs = json.Split(',');

        foreach (string pair in pairs)
        {
            // Split each pair into key and value
            string[] keyValue = pair.Split(':');

            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim().Trim('\"');
                string value = keyValue[1].Trim().Trim('\"');

                // Check if the value is an integer or a string
                int intValue;
                if (int.TryParse(value, out intValue))
                {
                    // If the value can be parsed as an integer, store it as an integer
                    jsonObject[key] = intValue;
                }
                else
                {
                    // Otherwise, store it as a string
                    jsonObject[key] = value;
                }
            }
        }

        return jsonObject;
    }
}
