using System;
using UnityEngine;


public enum WeaveMode
{
    Dobby,
    Jacquard
}

[Serializable]
public class WeaveData
{
    public string weaveName;
    public string weaveCode;
    public int coiCount; 
    public int rowCount;
    public int[] cells;
    public string[] warpColorNames;
    public string[] weftColorNames;
    public float[] warpThickness; // 0.1 ~ 1.0
    public float[] weftThickness; // 0.1 ~ 1.0
    public string savedAt;
}

