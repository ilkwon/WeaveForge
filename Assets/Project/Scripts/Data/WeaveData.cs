using System;
using UnityEngine;

[Serializable]
public class WeaveData
{
    public string weaveName;
    public string weaveCode;
    public int repeatX;
    public int repeatY;
    public int[] cells;
    public string[] warpColorNames;
    public string[] weftColorNames;
    public float[] warpThickness; // 0.1 ~ 1.0
    public float[] weftThickness; // 0.1 ~ 1.0
    public string savedAt;
}