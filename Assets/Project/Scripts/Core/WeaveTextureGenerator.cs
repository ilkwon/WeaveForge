using UnityEngine;

public class WeaveTextureGenerator : MonoBehaviour
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {

  }

  public static Texture2D GenerateDiffuse(WeaveData data)
  {
    var texSize = data.repeatX * data.repeatY;
    Texture2D dest = new(data.repeatX, data.repeatY)
    {
      filterMode = FilterMode.Point,
    };

    var width = data.repeatX;
    var height = data.repeatY;
    string[] warpColors = ColorSettings.LoadWarpColors(width);
    string[] weftColors = ColorSettings.LoadWeftColors(height);
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int cell = data.cells[y * width + x];
        Color color = cell == 1
        ? ColorPalette.GetColor(warpColors[x])
        : ColorPalette.GetColor(weftColors[y]);
        int texY = height - 1 - y;
        dest.SetPixel(x, texY, color);
      }
    }

    dest.Apply();
    return dest;
  }
  //---------------------------------------------------------------------------
  public static Texture2D GenerateHeigh(WeaveData data)
  {
    var texSize = data.repeatX * data.repeatY;
    Texture2D dest = new(data.repeatX, data.repeatY)
    {
      filterMode = FilterMode.Point,
    };

    var width = data.repeatX;
    var height = data.repeatY;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int cell = data.cells[y * data.repeatX + x];
        float valueHeight = cell == 1 ? 1.0f : 0.0f;
        Color color = new(valueHeight, valueHeight, valueHeight);
        int texY = height - 1 - y;
        dest.SetPixel(x, texY, color);
      }
    }

    dest.Apply();
    return dest;
  }
  //---------------------------------------------------------------------------
}
