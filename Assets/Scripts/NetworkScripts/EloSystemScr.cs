using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EloSystemScr : MonoBehaviour
{
    private const string ServerUrl = "https://airclashserver.onrender.com/mm/calculateElo";

    [System.Serializable]
    public class EloRequestData
    {
        public double ratingA;
        public double ratingB;
        public int matchesA;
        public int matchesB;
        public int goalsA;
        public int goalsB;
    }

    [System.Serializable]
    public class EloResponseData
    {
        public string status;
        public int newRatingA;
        public int newRatingB;
        public int deltaA;
        public int deltaB;
    }

    public void CalculateNewRatings(double ratingA, double ratingB, int matchesA, int matchesB, int goalsA, int goalsB, Action<EloResponseData> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(SendEloCalculationRoutine(ratingA, ratingB, matchesA, matchesB, goalsA, goalsB, onSuccess, onError));
    }

    private IEnumerator SendEloCalculationRoutine(double ratingA, double ratingB, int matchesA, int matchesB, int goalsA, int goalsB, Action<EloResponseData> onSuccess, Action<string> onError)
    {
        EloRequestData requestData = new EloRequestData
        {
            ratingA = ratingA,
            ratingB = ratingB,
            matchesA = matchesA,
            matchesB = matchesB,
            goalsA = goalsA,
            goalsB = goalsB
        };

        string jsonBody = JsonUtility.ToJson(requestData);

        using(UnityWebRequest www = new UnityWebRequest(ServerUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-game-secret", GameConfig.ApiSecret);

            Debug.Log("[EloSystemScr] Отправка запроса на Render.com...");
            yield return www.SendWebRequest();
            Debug.Log($"[EloSystemScr] Ответ получен! Результат: {www.result}");

            if(www.result == UnityWebRequest.Result.Success)
            {
                EloResponseData response = JsonUtility.FromJson<EloResponseData>(www.downloadHandler.text);
                if(response.status == "success")
                {
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke("[EloSystemScr] Не удалось получить эло с сервера");
                }
            }
            else
            {
                onError?.Invoke(www.error);
            }
        }
    }
}