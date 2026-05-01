using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class WeaveSettings
{
  [TitleGroup("반복 설정")]
  [LabelText("경사 반복수"), Range(1, 32)]
  public int warpRepeat = 4;

  [TitleGroup("반복 설정")]
  [LabelText("위사 반복수"), Range(1, 32)]
  public int weftRepeat = 4;

  [TitleGroup("반복 설정")]
  [LabelText("경사 반복수"), Range(2, 64)]
  public int colCount = 8;
  [TitleGroup("반복 설정")]
  [LabelText("위사 반복수"), Range(2, 64)]
  public int rowCount = 8;

  [TitleGroup("렌더링 설정")]
  [LabelText("셀 가로 크기 (픽셀)"), Range(4, 128)]
  public int pixelsPerWarp = 64;  // 경사 1올당 픽셀 수

  [TitleGroup("렌더링 설정")]
  [LabelText("셀 세로 크기 (픽셀)"), Range(4, 128)]
  public int pixelsPerWeft = 88;  // 위사 1올당 픽셀 수

  [TitleGroup("원사 설정")]
  [LabelText("번수 (Ne)"), Range(10, 120)]
  public int yarnCount = 40;

  [TitleGroup("원사 설정")]
  [LabelText("재질")]
  public YarnMaterial material = YarnMaterial.Cotton;

  [TitleGroup("밀도 설정")]
  [LabelText("경사 밀도 (EPI)"), Range(40, 200)]
  public int epi = 100;

  [TitleGroup("밀도 설정")]
  [LabelText("위사 밀도 (PPI)"), Range(40, 200)]
  public int ppi = 72;

  [TitleGroup("렌더링 설정")]
  [LabelText("크림프 강도"), Range(0f, 0.3f)]
  public float crimpStrength = 0.1f;

  //-------------------------------------------------------------------------
  [TitleGroup("파생 계산값 (읽기 전용)")]
  [ShowInInspector, ReadOnly, LabelText("실 직경 (mm)")]
  //public float YarnDiameterMm => 25.4f / (yarnCount * Mathf.Sqrt(yarnCount) * 0.9f);      
  //public float YarnDiameterMm => 25.4f / (28f * Mathf.Sqrt(yarnCount));
  public float YarnDiameterMm => 25.4f / (GetPeirceConstant() * Mathf.Sqrt(yarnCount));

  [ShowInInspector, ReadOnly, LabelText("경사 피치 (mm)")]
  public float WarpPitchMm => 25.4f / epi;

  [ShowInInspector, ReadOnly, LabelText("위사 피치 (mm)")]
  public float WeftPitchMm => 25.4f / ppi;

  [ShowInInspector, ReadOnly, LabelText("커버리지 (%)")]
  public float CoveragePercent => Mathf.Clamp01(YarnDiameterMm / WarpPitchMm) * 100f;

  [ShowInInspector, ReadOnly, LabelText("Tiling X (1m 기준)")]
  public float TilingX => 1000f / (WarpPitchMm * colCount);

  [ShowInInspector, ReadOnly, LabelText("Tiling Y (1m 기준)")]
  public float TilingY => 1000f / (WeftPitchMm * rowCount);


  private float GetPeirceConstant()
  {
    return material switch
    {
      YarnMaterial.Cotton => 28.0f,
      YarnMaterial.Polyester => 25.9f,
      YarnMaterial.Wool => 25.0f,
      YarnMaterial.Silk => 30.0f,
      YarnMaterial.Linen => 27.0f,
      _ => 28.0f
    };
  }


}

//-----------------------------------------------------------------------------
public enum YarnMaterial
{
  Cotton,
  Polyester,
  Silk,
  Wool,
  Linen
}