using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeaveRenderView : MonoBehaviour
{
  [SerializeField] private Material weaveMaterial;

  [ShowInInspector, PreviewField(150)]
  public Texture2D DiffuseTex{ get; private set; }
  [ShowInInspector, PreviewField(150)]
  public Texture2D NormalTex{ get; private set; }
  [ShowInInspector, PreviewField(150)]
  public Texture2D RoughnessTex{ get; private set; }

  //-------------------------------------------------------------------------
  private void Start()
  {
    WeaveDocumentManager.Instance.OnDocumentChanged += OnDocumentChanged;
    if (WeaveDocumentManager.Instance.CurrentWeaveData != null)
      OnDocumentChanged(WeaveDocumentManager.Instance.CurrentWeaveData);  
  }

  private void OnDocumentChanged(WeaveData data)
  {
    var settings = WeaveDocumentManager.Instance.CurrentWeaveSettings;
    DiffuseTex = WeaveTextureGenerator.GenerateDiffuse(data, settings);

    var height = WeaveTextureGenerator.GenerateHeightUpscale(data, settings);
    NormalTex = WeaveTextureGenerator.GenerateNormal(height);
    RoughnessTex = WeaveTextureGenerator.GenerateRoughness(height);

    if (weaveMaterial != null)
    {
      weaveMaterial.SetTexture("_DiffuseTex", DiffuseTex);
      //weaveMaterial.SetTextureScale("_DiffuseTex", 
      //  new Vector2(settings.TilingX, settings.TilingY));
      //weaveMaterial.SetVector("_Tiling", 
      //  new Vector4(settings.TilingX, settings.TilingY, 0, 0));
      float tilingX = 1000f / (data.colCount * settings.WarpPitchMm);
      float tilingY = 1000f / (data.rowCount * settings.WeftPitchMm);
      weaveMaterial.SetVector("_Tiling", new Vector4(tilingX, tilingY, 0, 0));  
      weaveMaterial.SetTexture("_NormalTex", NormalTex);
      weaveMaterial.SetTexture("_RoughnessTex", RoughnessTex);
    }
  }
}
