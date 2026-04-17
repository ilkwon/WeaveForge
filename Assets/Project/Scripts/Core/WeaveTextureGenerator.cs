using System;
using UnityEngine;

public class WeaveTextureGenerator : MonoBehaviour
{
  //---------------------------------------------------------------------------
  public static Texture2D GenerateDiffuse(WeaveData data)
  {
    Texture2D dest = new(data.coiCount, data.rowCount)
    {
      filterMode = FilterMode.Point,
    };

    var width = data.coiCount;
    var height = data.rowCount;
    Color32[] pixels = new Color32[width * height];
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

        pixels[texY * width + x] = color;
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }
  //---------------------------------------------------------------------------
  public static Texture2D GenerateHeigh(WeaveData data)
  {
    Texture2D dest = new(data.coiCount, data.rowCount)
    {
      filterMode = FilterMode.Point,
    };

    var width = data.coiCount;
    var height = data.rowCount;
    Color32[] pixels = new Color32[width * height];

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        int cell = data.cells[y * width + x];
        float val = cell == 1 ? 1.0f : 0.0f;
        Color color = new(val, val, val);
        int texY = height - 1 - y;

        pixels[texY * width + x] = color;
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }
  //--------------------------------------------------------------
  public static Texture2D GenerateHeightUpscale(WeaveData data)
  {
    int cellSize = 16;
    int width = data.coiCount * cellSize;
    int height = data.rowCount * cellSize;
    Texture2D dest = new(width, height)
    {
      filterMode = FilterMode.Point,
    };

    Color32[] pixels = new Color32[width * height];

    for (int cy = 0; cy < data.rowCount; cy++)
    {
      for (int cx = 0; cx < data.coiCount; cx++)
      {
        int cell = data.cells[cy * data.coiCount + cx];

        for (int py = 0; py < cellSize; py++)
        {
          for (int px = 0; px < cellSize; px++)
          {
            // 정규화 : cos에 쓰기위해 0부터 1사이값으로 변환
            // cellSize를 0 ~ 15 -> 0 ~ 1

            // 중앙을 0.5 로 맞추려면
            // px 범위를 0~cellSize 가 아니라
            // 0~1 을 셀 중앙 기준으로 매핑
            float u = ((float)px + .5f) / cellSize;
            float v = ((float)py + .5f) / cellSize;
            /// float curvX = (1 - Mathf.Cos(u * Mathf.PI)) /2;
            /// float curvY = (1 - Mathf.Cos(v * Mathf.PI)) /2;
            float sinU = Mathf.Sin(u * Mathf.PI);
            float sinV = Mathf.Sin(v * Mathf.PI);
            // cell == 1 (경사가 위일때)
            // cell == 0 (위사가 위일때)
            float h = cell == 1 ? sinU : sinV;

            int texX = cx * cellSize + px;
            int texY = height - 1 - (cy * cellSize + py);
            Color c = new(h, h, h);
            pixels[texY * width + texX] = c;

          }
        }
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }

  //---------------------------------------------------------------------------
  // 가로 방향 (gx)  |  세로 방향 (gy)
  // -1  0  +1       |  -1  -2  -1
  // -2  0  +2       |   0   0   0
  // -1  0  +1       |  +1  +2  +1

  // [좌표계 주의]
  // cells[] 배열: Y=0 이 위 → 사람이 보는 조직도 방향 (Top-Left 원점)
  // 실제 직기:    Y=0 이 아래 → 천이 아래서 위로 짜여나옴 (Bottom-Left 원점)
  // WIF 산업표준: Y=0 이 아래 (Bottom-Left 원점)
  // Unity 텍스처: Y=0 이 아래 (Bottom-Left 원점)
  // → texY = height - 1 - y 로 Y flip 적용
  // → flip 부작용으로 Sobel gy 방향 반전
  // → nx = gx, ny = gy 로 보정
  // → 추후 cells[] 를 Bottom-Left 기준으로 전환 시 자연스럽게 해결
  public static Texture2D GenerateNormal(Texture2D srcHeightMap, float strength = 0.3f)
  {
    Texture2D dest = new(srcHeightMap.width, srcHeightMap.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = srcHeightMap.width;
    var height = srcHeightMap.height;
    Color32[] pixels = new Color32[width * height];
    Color32[] srcPixels = srcHeightMap.GetPixels32();

    for (var y = 0; y < height; y++)
    {
      for (var x = 0; x < width; x++)
      {
        // p 값을 기준으로 -1, +1 해도 바운더리를 벗어나지 않게

        /* 제거..
          Seam 현상
          → 대각선 방향으로 희미한 격자선 보임
          → 텍스처 반복 경계에서 발생
          → Clamp 로 인한 Sobel 오류
         uv 타일링으 높여서 repeat 반복수를 높이면 격자 경계가 도드라지게 보인다.         
        int x0 = Mathf.Clamp(x - 1, 0, width - 1);
        int x1 = Mathf.Clamp(x + 1, 0, width - 1);
        int y0 = Mathf.Clamp(y - 1, 0, height - 1);
        int y1 = Mathf.Clamp(y + 1, 0, height - 1);
        */

        // 수정 코드
        int x0 = (x - 1 + width) % width;
        int x1 = (x + 1) % width;
        int y0 = (y - 1 + height) % height;
        int y1 = (y + 1) % height;

        // rgb 모두 값이 같아서 r 하나만.

        float tl = srcPixels[y1 * width + x0].r / 255f;
        float tm = srcPixels[y1 * width + x].r / 255f;
        float tr = srcPixels[y1 * width + x1].r / 255f;

        float ml = srcPixels[y * width + x0].r / 255f;
        float mm = srcPixels[y * width + x].r / 255f;
        float mr = srcPixels[y * width + x1].r / 255f;

        float bl = srcPixels[y0 * width + x0].r / 255f;
        float bm = srcPixels[y0 * width + x].r / 255f;
        float br = srcPixels[y0 * width + x1].r / 255f;
        // sobel filter
        float gx = (-1 * tl) + (0 * tm) + (1 * tr)
                 + (-2 * ml) + (0 * mm) + (2 * mr)
                 + (-1 * bl) + (0 * bm) + (1 * br);
        float gy = (-1 * tl) + (-2 * tm) + (-1 * tr)
                 + (0 * ml) + (0 * mm) + (0 * mr)
                 + (1 * bl) + (2 * bm) + (1 * br);

        float nx = -gx;
        float ny = gy;
        float nz = 1.0f / strength;

        Vector3 normal = new Vector3(nx, ny, nz).normalized;
        float r = (normal.x + 1f) / 2;
        float g = (normal.y + 1f) / 2;
        float b = (normal.z + 1f) / 2;
        Color c = new(r, g, b);
        pixels[y * width + x] = c;
        //dest.SetPixel(x, y, c);
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }

  //---------------------------------------------------------------------------
  public static Texture2D GenerateRoughness(Texture2D heightMap,
    float minRoughness = 0.2f, float maxRoughness = 0.9f)
  {
    var src = heightMap;
    Texture2D dest = new(src.width, src.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = src.width;
    var height = src.height;
    Color32[] pixels = new Color32[width * height];
    Color32[] srcPixels = src.GetPixels32();
    var min = minRoughness;
    var max = maxRoughness;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        //var h = src.GetPixel(x, y).r;
        var h = srcPixels[y * width + x].r / 255f;  // 0~1 정규화
        float r = (1 - h) * (max - min) + min;
        //Color roughness = new(r, r, r);
        pixels[y * width + x] = new Color32(
          (byte)(r* 255),       // R
          (byte)(r* 255),       // G
          (byte)(r* 255),       // B
          255   // 알파 고정.
        );
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }
  
  //---------------------------------------------------------------------------
  public static Texture2D GenerateMetallicGloss(Texture2D heightMap, float minRoughness = 0.4f, float maxRoughness = 0.8f)
  {
     var src = heightMap;
    Texture2D dest = new(src.width, src.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = src.width;
    var height = src.height;
    Color32[] pixels = new Color32[width * height];
    Color32[] srcPixels = src.GetPixels32();
    var min = minRoughness;
    var max = maxRoughness;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {        
        // 높이가 높은 곳(실 위) → 러프니스 낮음 → Smoothness 높음 → 더 매끄럽게 반짝임.  실위가 반짝거림.
        // 실 위 (볼록한 부분):
        //   Height 높음 → Roughness 낮음 → Smoothness 높음 → 매끄럽고 반짝임
        // 
        // 실 사이 (교차점, 눌린 부분):
        //   Height 낮음 → Roughness 높음 → Smoothness 낮음 → 거칠고 무광
        var h = srcPixels[y * width + x].r / 255f;  // 0~1 정규화
        float r = (1 - h) * (max - min) + min;  // 러프니스
                      
        pixels[y * width + x] = new Color32(
          0,                           // R = Metallic 없음
          0,                           // G
          0,                           // B
          (byte)((1f - r) * 255)       // A = Smoothness = 1 - 러프니스
        );
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply();
    return dest;
  }
}
