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
    private double eyeRot_X = 0;
    /// <summary>
    /// Y 축 기준 눈의 회전 값
    /// </summary>
    private double eyeRot_Y = 0;
    /// <summary>
    /// Z 축 기준 눈의 회전 값
    /// </summary>
    private double eyeRot_Z = 0;

    [SerializeField] private GameObject controller;
    private double controllerPos_X;
    private double controllerPos_Y;
    private double controllerPos_Z;
            
    private double controllerRot_X;
    private double controllerRot_Y;
    private double controllerRot_Z;

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
        eyeRot_X = Math.Round(targetEye.transform.localEulerAngles.x < 180f ? targetEye.transform.localEulerAngles.x : targetEye.transform.localEulerAngles.x, 3);
        eyeRot_Y = Math.Round(targetEye.transform.localEulerAngles.y < 180f ? targetEye.transform.localEulerAngles.y : targetEye.transform.localEulerAngles.y, 3);
        eyeRot_Z = Math.Round(targetEye.transform.localEulerAngles.z < 180f ? targetEye.transform.localEulerAngles.z : targetEye.transform.localEulerAngles.z, 3);

        eyeText.Clear();
        eyeText.AppendLine($"<size=0.12><<b>{targetEye.name}</b>></size> Rotation value(axis)\n");
        eyeText.AppendLine($"<color=green>X : {eyeRot_X}</color>");
        eyeText.AppendLine($"<color=green>Y : {eyeRot_Y}</color>");
        eyeText.AppendLine($"<color=green>Z : {eyeRot_Z}</color>");

        // 콘트롤러 값 추출
        controllerPos_X = Math.Round(controller.transform.position.x, 3);
        controllerPos_Y = Math.Round(controller.transform.position.y, 3);
        controllerPos_Z = Math.Round(controller.transform.position.z, 3);

        controllerRot_X = Math.Round(controller.transform.localEulerAngles.x < 180f ? controller.transform.localEulerAngles.x : controller.transform.localEulerAngles.x, 3);
        controllerRot_Y = Math.Round(controller.transform.localEulerAngles.y < 180f ? controller.transform.localEulerAngles.y : controller.transform.localEulerAngles.y, 3);
        controllerRot_Z = Math.Round(controller.transform.localEulerAngles.z < 180f ? controller.transform.localEulerAngles.z : controller.transform.localEulerAngles.z, 3);


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
