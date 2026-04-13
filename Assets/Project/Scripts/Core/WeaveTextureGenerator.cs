using System.Security.Cryptography;
using UnityEngine;

public class WeaveTextureGenerator : MonoBehaviour
{  
  //---------------------------------------------------------------------------
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
        float val = cell == 1 ? 1.0f : 0.0f;
        Color color = new(val, val, val);
        int texY = height - 1 - y;
        dest.SetPixel(x, texY, color);
      }
    }

    dest.Apply();
    return dest;
  }
  //---------------------------------------------------------------------------
    // 가로 방향 (gx)  |  세로 방향 (gy)
    // -1  0  +1       |  -1  -2  -1
    // -2  0  +2       |   0   0   0
    // -1  0  +1       |  +1  +2  +1
  public static Texture2D GenerateNormal(Texture2D srcHeightMap, float strength = 2f)
  {
    var texSize = srcHeightMap.width * srcHeightMap.height;
    Texture2D dest = new(srcHeightMap.width, srcHeightMap.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = srcHeightMap.width;
    var height = srcHeightMap.height;
    
    for (var y = 0; y<height; y++)
    {
      for (var x = 0; x<width; x++)
      {
        // p 값을 기준으로 -1, +1 해도 바운더리를 벗어나지 않게
        int x0 = Mathf.Clamp(x-1, 0, width-1);
        int x1 = Mathf.Clamp(x+1, 0, width-1);
        int y0 = Mathf.Clamp(y-1, 0, height-1);
        int y1 = Mathf.Clamp(y+1, 0, height-1);
        // rgb 모두 값이 같아서 r 하나만.
        float tl  = srcHeightMap.GetPixel(x0, y1).r;
        float tm  = srcHeightMap.GetPixel( x, y1).r;
        float tr  = srcHeightMap.GetPixel(x1, y1).r;

        float ml  = srcHeightMap.GetPixel(x0, y).r;
        float mm  = srcHeightMap.GetPixel( x, y).r;
        float mr  = srcHeightMap.GetPixel(x1, y).r;
        
        float bl  = srcHeightMap.GetPixel(x0, y0).r;
        float bm  = srcHeightMap.GetPixel( x, y0).r;
        float br  = srcHeightMap.GetPixel(x1, y0).r;
        // sobel filter
        float gx = (-1*tl) + ( 0*tm) + ( 1*tr)
                 + (-2*ml) + ( 0*mm) + ( 2*mr)
                 + (-1*bl) + ( 0*bm) + ( 1*br);
        float gy = (-1*tl) + (-2*tm) + (-1*tr)
                 + ( 0*ml) + ( 0*mm) + ( 0*mr)
                 + ( 1*bl) + ( 2*bm) + ( 1*br);
        
        float nx = -gx;
        float ny = -gy;
        float nz = 1.0f / strength;

        Vector3 normal = new Vector3(nx, ny, nz).normalized;
        float r = (normal.x+1f )/2;
        float g = (normal.y+1f )/2;
        float b = (normal.z+1f )/2;
        Color c = new (r,g,b);
        
        dest.SetPixel(x, y, c);        
      }
    }
    
    dest.Apply();
    return dest;
  }
}
