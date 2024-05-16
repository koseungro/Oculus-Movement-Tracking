using System.Linq;
using DilmerGames.Core.Singletons;
using TMPro;
using UnityEngine;
using System;
using System.Text;



public class Logger : Singleton<Logger>
{
    [SerializeField]
    private TextMeshProUGUI debugAreaText = null;

    [SerializeField]
    private bool enableDebug = false;

    [SerializeField]
    private int maxLines = 15;

    StringBuilder eyeText = new StringBuilder();
    StringBuilder faceText = new StringBuilder();
    StringBuilder controllerText = new StringBuilder();

    /// <summary>
    /// 눈 Game Object
    /// </summary>
    [SerializeField]
    private GameObject targetEye;
    /// <summary>
    /// X 축 기준 눈의 회전 값
    /// </summary>
    private float eyeRot_X = 0;
    /// <summary>
    /// Y 축 기준 눈의 회전 값
    /// </summary>
    private float eyeRot_Y = 0;

    [SerializeField] private GameObject controller;

    void Awake()
    {
        if (debugAreaText == null)
        {
            debugAreaText = GetComponent<TextMeshProUGUI>();
        }
        //debugAreaText.text = string.Empty;
    }

    void OnEnable()
    {
        debugAreaText.enabled = enableDebug;
        enabled = enableDebug;

        if (enabled)
        {
            //debugAreaText.text += $"<color=\"white\">{DateTime.Now.ToString("HH:mm:ss.fff")} {this.GetType().Name} enabled</color>\n";1
        }
    }

    private void Update()
    {
        SetLogText();

    }

    private void SetLogText()
    {
        // 눈 회전 값 추출
        eyeRot_X = MathF.Floor((targetEye.transform.localEulerAngles.x < 180f ? targetEye.transform.localEulerAngles.x : targetEye.transform.localEulerAngles.x - 360f) * 1000) * 0.001f;
        eyeRot_Y = MathF.Floor((targetEye.transform.localEulerAngles.y < 180f ? targetEye.transform.localEulerAngles.y : targetEye.transform.localEulerAngles.y - 360f) * 1000) * 0.001f;

        eyeText.Clear();
        eyeText.AppendLine($"<size=0.12><<b>{targetEye.name}</b>></size>  rotation value(axis)\n");
        eyeText.AppendLine($"<color=green>X : {eyeRot_X}  Y : {eyeRot_Y}</color>");

        debugAreaText.text = eyeText.ToString();
    }
    public void Clear() => debugAreaText.text = string.Empty;

    public void LogInfo(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"green\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    public void LogInfo(GameObject obj)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"green\">{DateTime.Now.ToString("HH:mm:ss.fff")} Name: {obj.name} Id: {obj.GetHashCode()}</color>\n";
    }

    public void LogError(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"red\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    public void LogWarning(string message)
    {
        ClearLines();
        debugAreaText.text += $"<color=\"yellow\">{DateTime.Now.ToString("HH:mm:ss.fff")} {message}</color>\n";
    }

    private void ClearLines()
    {
        if (debugAreaText.text.Split('\n').Count() >= maxLines)
        {
            debugAreaText.text = string.Empty;
        }
    }
}
