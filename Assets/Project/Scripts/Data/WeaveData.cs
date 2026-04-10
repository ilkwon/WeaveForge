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
    // 색상 이름 배열 (warp, weft) - 실제 색상은 ColorPalette에서 이름으로 찾아서 사용
    public string[] warpColorNames;
    public string[] weftColorNames;
    public string savedAt;
}