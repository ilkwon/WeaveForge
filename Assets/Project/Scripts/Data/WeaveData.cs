using System;
using UnityEngine;

[Serializable]
public class WeaveData
{
  public string weaveName;
  public string weaveCode;
  public int repeatX;
  public int repeatY;
  public int[] cells; // 1D 배열로 저장 (row-major order)
  public string savedAt;
}