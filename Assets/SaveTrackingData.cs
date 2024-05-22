using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using UnityEditor;

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
    [SerializeField]
    public static SkinnedMeshRenderer targetFaceRenderer;

    private Mesh faceSkinnedMesh;
    private int blendShapeCount;

    /// <summary>
    /// ������ �� [Blend �̸� - Value] ��Ʈ ����Ʈ
    /// </summary>
    private List<BlendValue> blendValue_List = new List<BlendValue>();

    public static List<bool> checkParameter_List = new List<bool>();

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

    //private void Reset()
    //{
    //    if (targetFaceRenderer == null)
    //        return;

    //    for (int i = 0; i < targetFaceRenderer.sharedMesh.blendShapeCount; i++)
    //    {
    //        checkParameter_List.Add(true);
    //    }

    //}

    /// <summary>
    /// Blend Parameter ����Ʈ �߰��� ������ - Editor���� �۵�
    /// </summary>
    static SaveTrackingData()
    {
        Debug.Log("?");
         if (targetFaceRenderer == null)
            return;

        Debug.Log("<color=yellow> Blend Parameter ����Ʈ �߰�</color>");
        for (int i = 0; i < targetFaceRenderer.sharedMesh.blendShapeCount; i++)
        {
            checkParameter_List.Add(true);
        }

    }

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

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            checkTracking = !checkTracking;

        if (Input.GetKeyDown(KeyCode.Space))
            WriteCSV(dataFolderName);

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
            double trackingValue = Math.Round(targetFaceRenderer.GetBlendShapeWeight(i), 3);
            blendDataLine.Append($"{trackingValue}, ");

            //blendValue.SetBlendDic(faceSkinnedMesh.GetBlendShapeName(i), (float)trackingValue);
        }

        trackingBuilder.AppendLine(blendDataLine.ToString());
        //blendValue_List.Add(blendValue);
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
                        EditorGUILayout.LabelField("Tracking Data üũ �ֱ� ��� ����", GUILayout.MaxWidth(220));
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

                    if (SaveTrackingData.targetFaceRenderer != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("", GUILayout.Width(10));
                                TrackingData.isFold = EditorGUILayout.Foldout(TrackingData.isFold, "<Blend Shape Parameter ���>", true);

                            }
                            EditorGUILayout.EndHorizontal();

                            if (TrackingData.isFold)
                            {
                                //EditorGUI.indentLevel++;
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    for (int i = 0; i < SaveTrackingData.targetFaceRenderer.sharedMesh.blendShapeCount; i++)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            EditorGUILayout.LabelField($"{SaveTrackingData.targetFaceRenderer.sharedMesh.GetBlendShapeName(i)}", GUILayout.MaxWidth(220));

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
