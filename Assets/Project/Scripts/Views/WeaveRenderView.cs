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
    var height = WeaveTextureGenerator.GenerateHeightUpscale(data);
    DiffuseTex = WeaveTextureGenerator.GenerateDiffuse(data);
    NormalTex = WeaveTextureGenerator.GenerateNormal(height);
    RoughnessTex = WeaveTextureGenerator.GenerateRoughness(height);

    if (weaveMaterial != null)
    {
      weaveMaterial.SetTexture("_DiffuseTex", DiffuseTex);
      weaveMaterial.SetTexture("_NormalTex", NormalTex);
      weaveMaterial.SetTexture("_RoughnessTex", RoughnessTex);
    }
  }
}
