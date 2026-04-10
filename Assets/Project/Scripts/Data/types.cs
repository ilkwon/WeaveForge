using System;
using UnityEngine;
//--------------------------------------------------------------------------
[Serializable]
public class UsefulColor
{
  public string colorName;
  public Color color;
  
  public UsefulColor(string name, Color col)
  {
    colorName = name;
    color = col;
  }
}

//--------------------------------------------------------------------------
// 유용한 색상 팔레트
public static class ColorPalette
{
  public static readonly UsefulColor[] Colors = new UsefulColor[]
  {
        new UsefulColor("White",  new Color(0.96f, 0.96f, 0.94f)),
        new UsefulColor("Ivory",  new Color(1.00f, 0.98f, 0.94f)),
        new UsefulColor("Beige",  new Color(0.78f, 0.72f, 0.60f)),
        new UsefulColor("Camel",  new Color(0.63f, 0.47f, 0.31f)),
        new UsefulColor("Brown",  new Color(0.35f, 0.24f, 0.17f)),
        new UsefulColor("Navy",   new Color(0.10f, 0.15f, 0.27f)),
        new UsefulColor("Black",  new Color(0.10f, 0.10f, 0.10f)),
        new UsefulColor("Gray",   new Color(0.53f, 0.53f, 0.50f)),
        new UsefulColor("Red",    new Color(0.63f, 0.19f, 0.13f)),
        new UsefulColor("Green",  new Color(0.16f, 0.35f, 0.23f)),
  };

  // 이름으로 색상 가져오기 (없으면 흰색 반환)
  public static Color GetColor(string name)
  {
    foreach (var c in Colors)
      if (c.colorName == name) return c.color;
    return Color.white;
  }

  //--------------------------------------------------------------------------
}