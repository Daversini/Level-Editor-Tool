#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LevelEditor : EditorWindow
{
    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow() => GetWindow<LevelEditor>("Level Editor");

    [SerializeField] private Color previewColor = Color.white;
    [SerializeField] private bool enablePrefabPreview = true;
    private Rect paletteMenuRect;
    private Vector2 scrollPos;
    [SerializeField] private string saveFolder;
    [SerializeField] private string saveFile;
    [SerializeField] private string loadFile;

    private SerializedObject serializedObject;
    private SerializedProperty previewColorProp;
    private SerializedProperty enablePrefabPreviewProp;
    private SerializedProperty saveFolderProp;
    private SerializedProperty saveFileProp;
    private SerializedProperty loadFileProp;

    private PrefabManager prefabManager;
    private PreviewManager previewManager;

    private void OnEnable()
    {
        prefabManager = new PrefabManager();
        previewManager = new PreviewManager
        {
            EnablePrefabPreview = enablePrefabPreview,
            PreviewColor = previewColor
        };
        InitializeLevelEditor();
    }

    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnGUI()
    {
        if (serializedObject == null) return;

        serializedObject.Update();
        DrawPreviewSettings();
        GUILayout.Space(10f);
        DrawSaveLoadSettings();

        if (serializedObject.ApplyModifiedProperties())
        {
            Repaint();
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        DrawPaletteMenu();
        previewManager.UpdatePreviewTransform(sceneView, prefabManager.SelectedPrefab);
        previewManager.DrawPrefabPreview();
        HandleInputEvents();
    }

    #region UI Drawing Methods

    private void DrawPreviewSettings()
    {
        EditorGUILayout.PropertyField(enablePrefabPreviewProp);
        if (!enablePrefabPreview) return;

        EditorGUI.indentLevel++;
        previewColorProp.colorValue = EditorGUILayout.ColorField("Preview Color", previewColorProp.colorValue);
        EditorGUI.indentLevel--;
    }

    private void DrawSaveLoadSettings()
    {
        EditorGUILayout.LabelField("Save/Load Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(saveFolderProp);
        EditorGUILayout.PropertyField(saveFileProp);
        if (GUILayout.Button("Save Current Level"))
            LevelSaveHandler.SaveCurrentLevel(saveFolder, saveFile);

        GUILayout.Space(10f);

        EditorGUILayout.PropertyField(loadFileProp);
        if (GUILayout.Button("Load Level"))
            LevelSaveHandler.LoadLevel(loadFile, prefabManager.Prefabs);
    }

    private void DrawPaletteMenu()
    {
        GUI.Window(0, paletteMenuRect, DrawPaletteMenuContent, "Prefab Palette");
    }

    private void DrawPaletteMenuContent(int id)
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                int previousIndex = prefabManager.SelectionGridIndex;
                prefabManager.SelectionGridIndex = GUILayout.SelectionGrid(prefabManager.SelectionGridIndex, prefabManager.PrefabIcons, 1);
                prefabManager.HandleSelectionChange(previousIndex);
                GUILayout.EndScrollView();
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Refresh List"))
                prefabManager.LoadPrefabs();
        }
    }

    #endregion

    #region Input Handling Methods

    private void HandleInputEvents()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.shift)
        {
            SpawnPrefab();
        }
        else if (currentEvent.type == EventType.KeyDown && currentEvent.shift)
        {
            if (currentEvent.keyCode == KeyCode.F)
            {
                enablePrefabPreview = !enablePrefabPreview;
                Repaint();
            }
            else if (currentEvent.keyCode == KeyCode.Space)
            {
                previewManager.UpdatePreviewRotation();
            }
        }
    }

    private void SpawnPrefab()
    {
        if (!enablePrefabPreview) return;

        GameObject obj = PrefabUtility.InstantiatePrefab(prefabManager.SelectedPrefab) as GameObject;
        Undo.RegisterCreatedObjectUndo(obj, "Spawn Object");
        obj.transform.SetPositionAndRotation(previewManager.PreviewPosition, previewManager.PreviewRotation);
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = previewColorProp.colorValue;
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeLevelEditor()
    {
        prefabManager.LoadPrefabs();
        this.StartCoroutine(CreatePaletteMenuRect());

        serializedObject = new SerializedObject(this);
        previewColorProp = serializedObject.FindProperty("previewColor");
        enablePrefabPreviewProp = serializedObject.FindProperty("enablePrefabPreview");
        saveFolderProp = serializedObject.FindProperty("saveFolder");
        saveFileProp = serializedObject.FindProperty("saveFile");
        loadFileProp = serializedObject.FindProperty("loadFile");

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private IEnumerator CreatePaletteMenuRect()
    {
        yield return null;
        float rectPosX = SceneView.lastActiveSceneView.camera.pixelWidth * 0.01f;
        float rectPosY = SceneView.lastActiveSceneView.camera.pixelHeight * 0.2f;
        paletteMenuRect = new Rect(rectPosX, rectPosY, 170, 500);
    }

    #endregion
}

#endif