using System;
using System.Security.Cryptography;
using Sirenix.Reflection.Editor;
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
  public static Texture2D GenerateHeightUpscale(WeaveData data)
  {
    int cellSize = 16;
    int width = data.repeatX * cellSize;
    int height = data.repeatY * cellSize;
    Texture2D dest = new(width, height)
    {
      filterMode = FilterMode.Point,
    };


    for (int cy = 0; cy < data.repeatY; cy++)
    {
      for (int cx = 0; cx < data.repeatX; cx++)
      {
        int cell = data.cells[cy * data.repeatX + cx];

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
            float curvX = Mathf.Sin(u * Mathf.PI);
            float curvY = Mathf.Sin(v * Mathf.PI);

            float heightVal = cell == 1 ? curvX : 1 - curvY;

            int texX = cx * cellSize + px;
            int texY = height - 1 - (cy * cellSize + py);
            Color c = new(heightVal, heightVal, heightVal);
            dest.SetPixel(texX, texY, c);
          }
        }
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
  public static Texture2D GenerateNormal(Texture2D srcHeightMap, float strength = 3f)
  {
    var texSize = srcHeightMap.width * srcHeightMap.height;
    Texture2D dest = new(srcHeightMap.width, srcHeightMap.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = srcHeightMap.width;
    var height = srcHeightMap.height;

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
        float tl = srcHeightMap.GetPixel(x0, y1).r;
        float tm = srcHeightMap.GetPixel(x, y1).r;
        float tr = srcHeightMap.GetPixel(x1, y1).r;

        float ml = srcHeightMap.GetPixel(x0, y).r;
        float mm = srcHeightMap.GetPixel(x, y).r;
        float mr = srcHeightMap.GetPixel(x1, y).r;

        float bl = srcHeightMap.GetPixel(x0, y0).r;
        float bm = srcHeightMap.GetPixel(x, y0).r;
        float br = srcHeightMap.GetPixel(x1, y0).r;
        // sobel filter
        float gx = (-1 * tl) + (0 * tm) + (1 * tr)
                 + (-2 * ml) + (0 * mm) + (2 * mr)
                 + (-1 * bl) + (0 * bm) + (1 * br);
        float gy = (-1 * tl) + (-2 * tm) + (-1 * tr)
                 + (0 * ml) + (0 * mm) + (0 * mr)
                 + (1 * bl) + (2 * bm) + (1 * br);

        float nx = -gx;
        float ny = -gy;
        float nz = 1.0f / strength;

        Vector3 normal = new Vector3(nx, ny, nz).normalized;
        float r = (normal.x + 1f) / 2;
        float g = (normal.y + 1f) / 2;
        float b = (normal.z + 1f) / 2;
        Color c = new(r, g, b);

        dest.SetPixel(x, y, c);
      }
    }

    dest.Apply();
    return dest;
  }

  //---------------------------------------------------------------------------
  public static Texture2D GenerateRoughness(Texture2D heightMap,
    float minRoughness = 0.4f, float maxRoughness = 0.8f)
  {
    var src = heightMap;
    Texture2D dest = new(src.width, src.height)
    {
      filterMode = FilterMode.Point,
    };

    var width = src.width;
    var height = src.height;
    var min = minRoughness;
    var max = maxRoughness;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        var h = src.GetPixel(x, y).r;
        float r = (1 - h) * (max - min) + min;
        Color roughness = new(r, r, r);
        dest.SetPixel(x, y, roughness);
      }
    }

    dest.Apply();
    return dest;
  }
  //---------------------------------------------------------------------------
  public static Texture2D GenerateHeightUpscalePress(WeaveData data,
    int cellSize = 32, float pressStrength = 0.3f)
  {
    int width = data.repeatX * cellSize;
    int height = data.repeatY * cellSize;

    var dest = new Texture2D(width, height)
    {
      filterMode = FilterMode.Point,
    };
    // 각 셀 순회.
    for (int cy = 0; cy < data.repeatY; cy++)
    {
      for (int cx = 0; cx < data.repeatX; cx++)
      {

        int cell = data.cells[cy * data.repeatX + cx];
        for (int py = 0; py < cellSize; py++)
        {
          for (int px = 0; px < cellSize; px++)
          {
            float u = (px + 0.5f) / cellSize;
            float v = (py + 0.5f) / cellSize;
            var hw = Mathf.Sin(u * Mathf.PI); // height Warp
            var hf = Mathf.Sin(v * Mathf.PI); // height Weft
            var press = Mathf.Min(hw, hf);
            var val = cell == 1 ? hw : 1 - hf;
            var h = Mathf.Lerp(val, press, pressStrength);
            int texX = cx * cellSize + px;
            int texY = height - 1 - (cy * cellSize + py);
            dest.SetPixel(texX, texY, new Color(h, h, h));
          }
        }
      }
    }

    dest.Apply();
    return dest;
  }

  public static Texture2D GenerateHeightUpscalePressEx(WeaveData data,
    int cellSize = 16, float pressStrength = 0.5f, float power = 0.8f)
  {
    int width = data.repeatX * cellSize;
    int height = data.repeatY * cellSize;
    var dest = new Texture2D(width, height) { filterMode = FilterMode.Bilinear };

    for (int cy = 0; cy < data.repeatY; cy++)
    {
      for (int cx = 0; cx < data.repeatX; cx++)
      {
        // 현재 셀의 상단 실 종류 (1: Warp/수직, 0: Weft/수평)
        int topThread = data.cells[cy * data.repeatX + cx];

        for (int py = 0; py < cellSize; py++)
        {
          for (int px = 0; px < cellSize; px++)
          {
            float u = (px + 0.5f) / cellSize; // 가로축 (0~1)
            float v = (py + 0.5f) / cellSize; // 세로축 (0~1)

            float h = 0;

            if (topThread == 1) // Warp(수직실)가 위에 있을 때
            {
              // 1. 단면 곡률 (가로 방향으로 볼록)
              float crossSection = Mathf.Sin(u * Mathf.PI);
              crossSection = Mathf.Pow(crossSection, power);

              // 2. 종단 곡률 (세로 방향으로 오르내림)
              // 교차점(0.5)에서 가장 높고 가장자리에선 낮아짐
              float path = Mathf.Lerp(0.5f, 1.0f, Mathf.Sin(v * Mathf.PI));

              h = crossSection * path;

              // 3. Press 효과: 높이가 일정 수준 이상일 때 납작하게 누름
              float threshold = 1.0f - (pressStrength * 0.5f);
              if (h > threshold)
              {
                h = Mathf.Lerp(h, threshold, pressStrength);
              }
            }
            else // Weft(수평실)가 위에 있을 때
            {
              // 1. 단면 곡률 (세로 방향으로 볼록)
              float crossSection = Mathf.Sin(v * Mathf.PI);
              crossSection = Mathf.Pow(crossSection, power);

              // 2. 종단 곡률 (가로 방향으로 오르내림)
              float path = Mathf.Lerp(0.5f, 1.0f, Mathf.Sin(u * Mathf.PI));

              h = crossSection * path;

              // 3. Press 효과
              float threshold = 1.0f - (pressStrength * 0.5f);
              if (h > threshold)
              {
                h = Mathf.Lerp(h, threshold, pressStrength);
              }
            }

            int texX = cx * cellSize + px;
            int texY = height - 1 - (cy * cellSize + py);
            dest.SetPixel(texX, texY, new Color(h, h, h));
          }
        }
      }
    }
    dest.Apply();
    return dest;
  }

  public static Texture2D GenerateHeightUpscalePressPro(WeaveData data,
    int cellSize = 16, float pressStrength = 0.3f)
  {
    int width = data.repeatX * cellSize;
    int height = data.repeatY * cellSize;

    var dest = new Texture2D(width, height)
    {
      // 포인트 필터보다 바이리니어 필터가 높이맵 생성 후 노말 변환 시 훨씬 부드럽습니다.
      filterMode = FilterMode.Bilinear, 
    };

    // 각 셀 순회
    for (int cy = 0; cy < data.repeatY; cy++)
    {
      for (int cx = 0; cx < data.repeatX; cx++)
      {
        int cell = data.cells[cy * data.repeatX + cx]; // 1: Warp(수직실 위), 0: Weft(수평실 위)

        for (int py = 0; py < cellSize; py++)
        {
          for (int px = 0; px < cellSize; px++)
          {
            float u = (px + 0.5f) / cellSize;
            float v = (py + 0.5f) / cellSize;
            float h = 0f;

            if (cell == 1) // Warp (수직실이 위)
            {
              // 1. 단면과 경로 계산
              float crossSection = Mathf.Sin(u * Mathf.PI); // 가로로 볼록
              float path = Mathf.Lerp(0.3f, 1.0f, Mathf.Sin(v * Mathf.PI)); // 아래에서 위로 올라오는 흐름
              h = crossSection * path;
            }
            else // Weft (수평실이 위)
            {
              // 1. 단면과 경로 계산
              float crossSection = Mathf.Sin(v * Mathf.PI); // 세로로 볼록
              float path = Mathf.Lerp(0.3f, 1.0f, Mathf.Sin(u * Mathf.PI)); // 좌우 흐름
              h = crossSection * path;
            }

            // 2. Press(눌림) 처리: 특정 높이 이상을 평평하게 깎아냄
            // pressStrength가 0이면 원래 형태, 1에 가까울수록 윗부분이 심하게 납작해짐
            float threshold = 1.0f - (pressStrength * 0.4f); // 깎이기 시작하는 높이 기준점
            if (h > threshold)
            {
               // 초과한 높이를 pressStrength에 따라 줄여서 납작하게 만듦
               float excess = h - threshold;
               h = threshold + (excess * (1.0f - pressStrength));
            }

            int texX = cx * cellSize + px;
            int texY = height - 1 - (cy * cellSize + py);    
            dest.SetPixel(texX, texY, new Color(h, h, h));
          }
        }
      }
    }
    
    dest.Apply();
    return dest;
  }
}
