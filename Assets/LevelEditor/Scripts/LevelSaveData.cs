using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSaveData
{
    public List<int> IDs { get; private set; }
    public List<Vector3> Positions { get; private set; }
    public List<Quaternion> Rotations { get; private set; }

    public LevelSaveData()
    {
        IDs = new List<int>();
        Positions = new List<Vector3>();
        Rotations = new List<Quaternion>();
    }
}