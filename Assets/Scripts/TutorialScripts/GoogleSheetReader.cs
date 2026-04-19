using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
// AI https://gemini.google.com/share/03886b738717
public class GoogleSheetReader : MonoBehaviour
{
    // Paste your published CSV URL here
    public string sheetURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vS0_hZ3F0hbX0W6UluFPqagRX76o1_C_MjZRToBaMjkonON9I9fpmpX2pyyf5azqzx6p2Wxh0RwXPUf/pub?gid=0&single=true&output=csv";

    void Start()
    {
        StartCoroutine(FetchData());
    }

    IEnumerator FetchData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(sheetURL))
        {
            // Send the request and wait for a response
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching data: " + www.error);
            }
            else
            {
                // Success! Get the downloaded text
                string csvData = www.downloadHandler.text;
                Debug.Log("Downloaded Data:\n" + csvData);
                
                // You can now parse this CSV data
                ParseCSV(csvData);
            }
        }
    }

    void ParseCSV(string data)
    {
        // Split the data by newlines to get rows
        string[] rows = data.Split('\n');

        foreach (string row in rows)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;

            // Split each row by commas to get cells
            string[] cells = row.Split(',');
            
            // Example: Print the first cell of each row
            Debug.Log("First column: " + cells[0]); 
        }
    }
}