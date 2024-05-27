using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEditor;
using TMPro;
using System.Linq;

[InitializeOnLoad]
public class SaveTrackingData : MonoBehaviour
{
    struct BlendValue
    {
        /// <summary>
        /// <Blend parameter name, ��> ��ųʸ� �迭
        /// </summary>
        public Dictionary<string, float> blendDic;

        public void SetBlendDic(string str, float value)
        {
            blendDic.Add(str, value);
        }
    }

    private string dataFolderName = "";
    private StringBuilder trackingBuilder = new StringBuilder();

    /// <summary>
    /// Tracking �����͸� ���� �������� üũ
    /// </summary>
    private bool checkTracking = false;

    /// <summary>
    /// Target�� �� �� Renderer
    /// </summary>    
    public SkinnedMeshRenderer targetFaceRenderer;

    public TextMeshProUGUI trackingInform_Text;

    private Mesh faceSkinnedMesh;
    private int blendShapeCount;

    /// <summary>
    /// ������ �� [Blend �̸� - Value] ��Ʈ ����Ʈ
    /// </summary>
    private List<BlendValue> blendValue_List = new List<BlendValue>();

    public List<bool> checkParameter_List;

    /// <summary>
    /// 
    /// </summary>
    [Tooltip("Ʈ��ŷ üũ �ֱ⸦ '��'�� �������� ����(false : fps)")]
    public bool trackingCycle_UseSecond = false;

    /// <summary>
    /// Tracking Data üũ �ֱ� �ð�
    /// </summary>
    public float trackingCheckTime = 0;
    private float curTime = 0;

    [Header("[Custom Editor Value]")]
    public bool isFold = false;


    private void Awake()
    {
        if (targetFaceRenderer != null)
        {
            faceSkinnedMesh = targetFaceRenderer.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            blendShapeCount = faceSkinnedMesh.blendShapeCount;
        }

    }

    void Start()
    {
        CreateDataFolder();

        if (targetFaceRenderer != null)
        {
            for (int i = 0; i < targetFaceRenderer.sharedMesh.blendShapeCount; i++)
            {
                checkParameter_List.Add(true);
            }
        }

    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"<color=yellow>Tracking Data Save ����</color>");
            SetTrackingLog();
            checkTracking = !checkTracking;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            WriteCSV(dataFolderName);
#endif

        if (checkTracking)
        {
            if (trackingCycle_UseSecond)
            {
                if (curTime > trackingCheckTime)
                {
                    curTime = 0;
                    UpdateTrackingData();
                }
                else
                {
                    curTime += Time.deltaTime;
                }
            }
            else
                UpdateTrackingData(); // ������ ������ üũ


        }
    }

    /// <summary>
    /// Tracking Data�� ���� �� CSV ������ �����մϴ�.
    /// </summary>
    public void WriteCSV(string path)
    {
        string dataFile = path + "/Tracking Data_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

        try
        {
            StreamWriter writer = new StreamWriter(dataFile, true, Encoding.UTF8);

            StringBuilder titleLine = new StringBuilder();

            for (int i = 0; i < blendShapeCount; i++)
            {
                titleLine.Append(faceSkinnedMesh.GetBlendShapeName(i) + ",");
            }

            writer.WriteLine(titleLine);

            writer.Write(trackingBuilder);
            writer.Close();

            trackingBuilder.Clear();

            Debug.Log($"<color=yellow>Tracking Data ���� : {dataFile}</color>");
        }
        catch (System.Exception)
        {
            Debug.Log($"<color=magenta>[SaveTrackingData/WriteCSV] ���� ���� </color>");
        }
    }

    /// <summary>
    /// Tracking Data�� String Builder ������ ���������� ������Ʈ �մϴ�
    /// </summary>
    private void UpdateTrackingData()
    {
        StringBuilder blendDataLine = new StringBuilder();
        //BlendValue blendValue = new BlendValue();

        for (int i = 0; i < blendShapeCount; i++)
        {
            if (checkParameter_List[i])
            {
                double trackingValue = Math.Round(targetFaceRenderer.GetBlendShapeWeight(i), 3);
                blendDataLine.Append($"{trackingValue},");

                //blendValue.SetBlendDic(faceSkinnedMesh.GetBlendShapeName(i), (float)trackingValue);
            }
            else
            {
                blendDataLine.Append($"Null,");
                //blendValue.SetBlendDic(faceSkinnedMesh.GetBlendShapeName(i), 999999);

            }
        }

        trackingBuilder.AppendLine(blendDataLine.ToString());
        //blendValue_List.Add(blendValue);
    }

    public void SetTrackingLog()
    {
        trackingInform_Text.text = $"<size=0.2><color=yellow><b>Face Tracking Start</b></color></size>\n\n" +
            $"Target Mesh : <color=yellow>[{targetFaceRenderer.name}]</color>" +
            $"Tracking Data Cycle : <color=yellow>[{(trackingCycle_UseSecond?trackingCheckTime:0)}]</color> Second\n" +
            $"Tracking Check Parameter Count : <color=yellow>[{checkParameter_List.Where(x=>x == true).Count()}/ {checkParameter_List.Count}]</color>";
    }

    /// <summary>
    /// ������ ���� ������ �����մϴ�.
    /// </summary>
    private void CreateDataFolder()
    {
        if (dataFolderName == "")
        {
#if UNITY_EDITOR
            dataFolderName = Application.dataPath + "/../Tracking Data";
#else
            dataFolderName = Application.persistentDataPath + "/Tracking Data";
#endif
            if (!Directory.Exists(dataFolderName))
            {
                Directory.CreateDirectory(dataFolderName);
                Debug.Log($"<color=yellow>Data ���� ���� : {dataFolderName}</color>");
            }

        }
    }
}


[CustomEditor(typeof(SaveTrackingData))]
public class SaveTrackingDataEditor : Editor
{
    private SaveTrackingData trackingData;
    private SaveTrackingData TrackingData
    {
        get
        {
            if (trackingData == null)
                trackingData = base.target as SaveTrackingData;

            return trackingData;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.objectFieldThumb);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Tracking Data üũ �ֱ�(��) ��� ����", GUILayout.MaxWidth(220));
                        TrackingData.trackingCycle_UseSecond = EditorGUILayout.Toggle(TrackingData.trackingCycle_UseSecond);
                    }
                    EditorGUILayout.EndHorizontal();

                    if (TrackingData.trackingCycle_UseSecond)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("üũ �ֱ�(��)", GUILayout.MaxWidth(100));
                            TrackingData.trackingCheckTime = EditorGUILayout.FloatField(TrackingData.trackingCheckTime, GUILayout.MaxWidth(50));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (TrackingData.targetFaceRenderer != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("", GUILayout.Width(10));
                                TrackingData.isFold = EditorGUILayout.Foldout(TrackingData.isFold, $"[{TrackingData.targetFaceRenderer.name}] Blend Shape Parameter ���", true);

                            }
                            EditorGUILayout.EndHorizontal();

                            if (TrackingData.isFold)
                            {
                                //EditorGUI.indentLevel++;
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    GUI.color = Color.yellow;
                                    EditorGUILayout.LabelField("�� �ش� ���� �Ķ���� �����͸� CSV ���Ͽ� ���Խ�ų�� üũ(Play ����� ���� ���� ����)", EditorStyles.boldLabel);
                                    GUI.color = Color.white;

                                    for (int i = 0; i < TrackingData.targetFaceRenderer.sharedMesh.blendShapeCount; i++)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUILayout.LabelField($"{TrackingData.targetFaceRenderer.sharedMesh.GetBlendShapeName(i)}", GUILayout.MaxWidth(320));

                                            if (TrackingData.checkParameter_List.Count == TrackingData.targetFaceRenderer.sharedMesh.blendShapeCount)
                                            {
                                                TrackingData.checkParameter_List[i] = EditorGUILayout.Toggle(TrackingData.checkParameter_List[i]);
                                            }

                                        }
                                        EditorGUILayout.EndHorizontal();

                                    }

                                }
                                EditorGUILayout.EndVertical();
                            }

                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(targets, "Changed Update Mode");
            EditorUtility.SetDirty(TrackingData);
        }
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }

}
