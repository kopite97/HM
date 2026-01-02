using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

public class SheetImporter : EditorWindow
{
    private const string URL_CLASS =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vRZ1VEaNQv3vkEh4uRtyyrHZQBr3gB7t1VHhh4m3sqdAG-9lLoxaV5oFQ-HjSNtN2YRCJaLOnc0_99T/pub?output=csv";

    private const string URL_MONSTER =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vRzOyyAJpytD8CbPZPyHgTUB1o2nIVvq1MiQOE5P2ISp35pZaYBRYQFMyiPgFLJQmdRRAVxyZOwV1Cn/pub?output=csv";

    private const string URL_SKILL =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vSDUVpGT_xz9h4KeOMUaqCqkLbEr3UwPtfBFENx4AqaktL8HJB0jBGa8AjGmIdcByBcGbb83Pk9GmCC/pub?output=csv";

    private const string URL_DUNGEON =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vQv8IjFrxELQTifAV8Qd24b3B5YlwHxdQeD70hpH_w8GhyBdybj-kEpnwxH_8Duzu8X1qZ75Dm9PjI1/pub?output=csv";

    private const string URL_RESISTANCECONFIG =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vTWUHVHOsfnW3evfFUBXmuMa2kwedMrIRBNEBNm1lvqAnr21YBaEUqVqaaXmMXATCmxI7FrLp2QJcK1/pub?output=csv";
    
    
    [MenuItem("Tools/Update Data (Google Sheets)")]
    public static void UpdateData()
    {
        string savePath = Application.dataPath + "/Resources/Data/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        DownloadCSV(URL_CLASS, savePath + "ClassData.csv");
        DownloadCSV(URL_MONSTER, savePath + "MonsterData.csv");
        DownloadCSV(URL_SKILL, savePath + "SkillData.csv");
        DownloadCSV(URL_DUNGEON, savePath + "DungeonData.csv");
        DownloadCSV(URL_RESISTANCECONFIG,savePath +"ResistanceConfig.csv");
        
        AssetDatabase.Refresh();
        Debug.Log("✅ 모든 데이터 다운로드 완료!");
    }

    private static void DownloadCSV(string url, string filePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        var operation = request.SendWebRequest();
        
        while(!operation.isDone) { } // 유니티용 동기 처리

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Error] {Path.GetFileName(filePath)} 다우론드 실패 : {request.error}");
        }
        else
        {
            File.WriteAllText(filePath, request.downloadHandler.text);
        }
    }

}