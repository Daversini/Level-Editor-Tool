using UnityEditor;
using UnityEngine;

public class PrefabManager
{
    public GameObject[] Prefabs { get; private set; }
    public GameObject SelectedPrefab { get; private set; }
    public Texture[] PrefabIcons { get; private set; }
    public int SelectionGridIndex { get; set; }

    public void LoadPrefabs()
    {
        Prefabs = Resources.LoadAll<GameObject>("SnappableObject/");
        PrefabIcons = new Texture[Prefabs.Length];
        for (int i = 0; i < Prefabs.Length; i++)
            PrefabIcons[i] = AssetPreview.GetAssetPreview(Prefabs[i]);

        SelectedPrefab = Prefabs.Length > 0 ? Prefabs[0] : null;
        SelectionGridIndex = 0;
    }

    public void HandleSelectionChange(int newIndex)
    {
        if (SelectionGridIndex == newIndex) return;
        SelectionGridIndex = newIndex;
        SelectedPrefab = Prefabs[SelectionGridIndex];
    }
}