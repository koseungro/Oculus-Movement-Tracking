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
    /// <summary>
    /// Z 축 기준 눈의 회전 값
    /// </summary>
    private float eyeRot_Z = 0;

    [SerializeField] private GameObject controller;
    private float controllerPos_X;
    private float controllerPos_Y;
    private float controllerPos_Z;

    private float controllerRot_X;
    private float controllerRot_Y;
    private float controllerRot_Z;

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
        eyeRot_Z = MathF.Floor((targetEye.transform.localEulerAngles.z < 180f ? targetEye.transform.localEulerAngles.z : targetEye.transform.localEulerAngles.z - 360f) * 1000) * 0.001f;

        eyeText.Clear();
        eyeText.AppendLine($"<size=0.12><<b>{targetEye.name}</b>></size> Rotation value(axis)\n");
        eyeText.AppendLine($"<color=green>X : {eyeRot_X}</color>");
        eyeText.AppendLine($"<color=green>Y : {eyeRot_Y}</color>");
        eyeText.AppendLine($"<color=green>Z : {eyeRot_Z}</color>");

        // 콘트롤러 값 추출
        controllerPos_X = MathF.Floor((controller.transform.position.x * 1000f)) * 0.001f;
        controllerPos_Y = MathF.Floor((controller.transform.position.y * 1000f)) * 0.001f;
        controllerPos_Z = MathF.Floor((controller.transform.position.z * 1000f)) * 0.001f;

        controllerRot_X = MathF.Floor((controller.transform.localEulerAngles.x < 180f ? controller.transform.localEulerAngles.x : controller.transform.localEulerAngles.x - 360f) * 1000) * 0.001f;
        controllerRot_Y = MathF.Floor((controller.transform.localEulerAngles.y < 180f ? controller.transform.localEulerAngles.y : controller.transform.localEulerAngles.y - 360f) * 1000) * 0.001f;
        controllerRot_Z = MathF.Floor((controller.transform.localEulerAngles.z < 180f ? controller.transform.localEulerAngles.z : controller.transform.localEulerAngles.z - 360f) * 1000) * 0.001f;


        controllerText.Clear();
        controllerText.AppendLine();
        controllerText.AppendLine($"<size=0.12><<b>{controller.name}</b>></size>\n");
        controllerText.AppendLine($"<size=0.11>Position</size>");
        controllerText.AppendLine($"<color=green>X : {controllerPos_X}</color>");
        controllerText.AppendLine($"<color=green>Y : {controllerPos_Y}</color>");
        controllerText.AppendLine($"<color=green>Z : {controllerPos_Z}</color>");

        controllerText.AppendLine($"<size=0.11>Rotation</size>");
        controllerText.AppendLine($"<color=green>X : {controllerRot_X}</color>");
        controllerText.AppendLine($"<color=green>Y : {controllerRot_Y}</color>");
        controllerText.AppendLine($"<color=green>Z : {controllerRot_Z}</color>");


        debugAreaText.text = eyeText.ToString() + controllerText.ToString();
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
