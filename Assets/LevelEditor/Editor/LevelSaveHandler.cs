using UnityEditor;
using UnityEngine;

public static class LevelSaveHandler
{
    public static void SaveCurrentLevel(string saveFolder, string saveFile)
    {
        LevelSaveData saveData = new LevelSaveData();
        foreach (SnappableObject snappableObj in GameObject.FindObjectsOfType<SnappableObject>())
        {
            saveData.IDs.Add(snappableObj.ObjID);
            saveData.Positions.Add(snappableObj.transform.position);
            saveData.Rotations.Add(snappableObj.transform.rotation);
        }
        JsonSaveHandler.Save(saveFolder, saveFile, saveData);
    }

    public static void LoadLevel(string loadFileName, GameObject[] prefabs)
    {
        LevelSaveData loadData = JsonSaveHandler.Load<LevelSaveData>(loadFileName);
        if (loadData == null)
        {
            Debug.LogError($"Level {loadFileName} loading failed");
            return;
        }

        int index = 0;
        foreach (int id in loadData.IDs)
        {
            GameObject obj = PrefabUtility.InstantiatePrefab(GetPrefabByID(id, prefabs)) as GameObject;
            obj.transform.SetPositionAndRotation(loadData.Positions[index], loadData.Rotations[index]);
            index++;
        }
    }

    private static GameObject GetPrefabByID(int id, GameObject[] prefabs)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab.GetComponent<SnappableObject>().ObjID == id)
                return prefab;
        }
        Debug.LogError($"Prefab with ID {id} not found");
        return null;
    }
}