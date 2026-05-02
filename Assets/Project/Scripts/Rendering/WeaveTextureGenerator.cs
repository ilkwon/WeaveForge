using System;
using Unity.ProjectAuditor.Editor.Core;
using UnityEngine;
/*
  WeaveTextureGenerator.cs
  - 위 조직 데이터를 기반으로 Diffuse, Normal, Roughness 텍스처를 생성하는 유틸리티 클래스.
  - WeaveRenderView에서 사용.

  [참고] Height 맵에서 Normal 맵 생성 시, Sobel 필터를 사용하여 높이의 변화량을 계산.
  - Sobel 필터는 각 픽셀 주변의 높이값을 이용하여
  - gx = (-1 * tl) + (0 * tm) + (1 * tr)
       + (-2 * ml) + (0 * mm) + (2 * mr)
       + (-1 * bl) + (0 * bm) + (1 * br);
  - gy = (-1 * tl) + (-2 * tm) + (-1 * tr)
       + (0 * ml) + (0 * mm) + (0 * mr)
       + (1 * bl) + (2 * bm) + (1 * br);
  - 여기서 tl, tm, tr, ml, mm, mr, bl, bm, br는 해당 픽셀 주변의 높이값입니다.
  - gx와 gy를 이용하여 법선 벡터를 계산하고, 이를 RGB로 변환하여 Normal 맵을 생성.    
  
  [참고] Roughness 맵 생성 시, 높이값을 이용하여 실 위는 매끄럽게, 실 사이 교차점은 거칠게 표현.
  - 높이가 높은 곳(실 위) → 러프니스 낮음 → Smoothness 높음 → 더 매끄럽게 반짝임.
  - 실 위 (볼록한 부분):
    Height 높음 → Roughness 낮음 → Smoothness 높음 → 매끄럽고 반짝임
  - 실 사이 (교차점, 눌린 부분):
    Height 낮음 → Roughness 높음 → Smoothness 낮음 → 거칠고 무광

  EPI (Ends Per Inch)  → 1인치 안에 경사(세로실)가 몇 올
  PPI (Picks Per Inch) → 1인치 안에 위사(가로실)가 몇 올
  40수 면 표준 평직 기준
  EPI = 100  → 1인치 안에 경사 100올
  PPI = 72   → 1인치 안에 위사 72올

  EPI가 PPI보다 높은 이유가 있습니다. 직기에서 경사는 항상 팽팽하게 당겨진 상태에서 
  위사를 "바디(Reed)"로 강하게 눌러 넣습니다. 그래서 경사가 위사보다 더 촘촘하게 배열됨.
  경사 1올 공간 = 25.4mm / 100 = 0.254mm
  위사 1올 공간 = 25.4mm / 72  = 0.353mm

  비율 = 위사 / 경사 = 0.353 / 0.254 ≈ 1.39
  위사가 경사보다 1.39배 넓은 공간을 차지합니다. 그래서 셀이 정사각형이 아니라 위사 방향으로
   약간 더 길어야 실제 원단 비율에 가깝움.  
*/
public class WeaveTextureGenerator : MonoBehaviour
{
  //---------------------------------------------------------------------------
  public static Texture2D GenerateDiffuse(WeaveData data, WeaveSettings settings = null)
  {
    if (data == null || data.colCount <= 0 || data.rowCount <= 0)
      return new Texture2D(1, 1);
    if (data.cells == null || data.cells.Length == 0)
      data.cells = new int[data.colCount * data.rowCount];
    if (data.warpColorNames == null || data.warpColorNames.Length == 0)
      data.warpColorNames = new string[data.colCount];
    if (data.weftColorNames == null || data.weftColorNames.Length == 0)
      data.weftColorNames = new string[data.rowCount];
    
    int pixelsPerWarp = settings != null ? settings.pixelsPerWarp : 16;
    int pixelsPerWeft = settings != null ? settings.PixelsPerWeft : 22;
    // 커버리지 계산
    float warpCoverage = settings != null ? settings.CoveragePercent / 100f : 0.565f;
    float weftCoverage = settings != null ? settings.WeftCoveragePercent / 100f : 0.406f;

    // 셀 반폭 픽셀
    float warpHalfPx = pixelsPerWarp * warpCoverage / 2f;
    float weftHalfPx = pixelsPerWeft * weftCoverage / 2f;

    float centerX = pixelsPerWarp / 2f;
    float centerY = pixelsPerWeft / 2f;
    
    // 밉맵 켜기.     
    Texture2D dest = new(data.colCount * pixelsPerWarp, data.rowCount * pixelsPerWeft, TextureFormat.RGBA32, true)
    {
      filterMode = FilterMode.Bilinear,
      anisoLevel = 9,  // 밉맵 품질 향상 위해 추가
    };

    var texWidth = dest.width;
    var texHeight = dest.height;

    Color32[] pixels = new Color32[texWidth * texHeight];



    for (int cy = 0; cy < data.rowCount; cy++)
    {
      for (int cx = 0; cx < data.colCount; cx++)
      {
        int cell = data.cells[cy * data.colCount + cx];

        Color warpColor = ColorPalette.GetColor(data.warpColorNames[cx]);
        Color weftColor = ColorPalette.GetColor(data.weftColorNames[cy]);
        
        for (int py = 0; py < pixelsPerWeft; py++){
          for (int px = 0; px < pixelsPerWarp; px++){
            float dx = Mathf.Abs(px - centerX);
            float dy = Mathf.Abs(py - centerY);

            Color color;
            if (cell == 1) // 경사가 위에 있는 경우
              color = dx <= warpHalfPx ? warpColor : Color.clear; // 셀 중앙에서 warpHalfPx 이내는 경사색, 그 외는 투명
            else // 위사가 위에 있는 경우
              color = dy <= weftHalfPx ? weftColor : Color.clear; // 셀 중앙에서 weftHalfPx 이내는 위사색, 그 외는 투명

            int texX = cx * pixelsPerWarp + px;
            int texY = (texHeight - 1) - (cy * pixelsPerWeft + py);
            pixels[texY * texWidth + texX] = color;
          }        
        }
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply(true);  // 밉맵 생성
    return dest;
  }
  //---------------------------------------------------------------------------
  public static Texture2D GenerateHeight(WeaveData data)
  {
    Texture2D dest = new(data.colCount, data.rowCount)
    {
      filterMode = FilterMode.Point,
    };

    var width = data.colCount;
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
  public static Texture2D GenerateHeightUpscale(WeaveData data, WeaveSettings settings = null)
  {
    if (data == null || data.colCount <= 0 || data.rowCount <= 0)
      return new Texture2D(1, 1);
    if (data.cells == null || data.cells.Length == 0)
      return new Texture2D(1, 1);

    
    var pixelsPerWarp = settings != null ? settings.pixelsPerWarp : 16;
    var pixelsPerWeft = settings != null ? settings.PixelsPerWeft : 22;
    
    
    float warpCoverage = settings != null ? settings.CoveragePercent / 100f : 0.565f;
    float weftCoverage = settings != null ? settings.WeftCoveragePercent / 100f : 0.406f;
    float warpHalfPx = pixelsPerWarp * warpCoverage / 2f;
    float weftHalfPx = pixelsPerWeft * weftCoverage / 2f;
    float centerX = pixelsPerWarp / 2f;
    float centerY = pixelsPerWeft / 2f;     

    int width = data.colCount * pixelsPerWarp;
    int height = data.rowCount * pixelsPerWeft;

    Texture2D dest = new(width, height, TextureFormat.RGBA32, true)
    {
      filterMode = FilterMode.Bilinear,
      anisoLevel = 9,  // 밉맵 품질 향상 위해 추가
    };

    Color32[] pixels = new Color32[width * height];

    for (int cy = 0; cy < data.rowCount; cy++)
    {
      for (int cx = 0; cx < data.colCount; cx++)
      {
        int cell = data.cells[cy * data.colCount + cx];

        for (int py = 0; py < pixelsPerWeft; py++)
        {
          for (int px = 0; px < pixelsPerWarp; px++)
          {
            float dx = Mathf.Abs(px - centerX);
            float dy = Mathf.Abs(py - centerY);
            float h;
            if (cell == 1) // 경사가 위에 있는 경우
            {
              if (dx <= warpHalfPx) // 셀 중앙에서 warpHalfPx 이내는 경사 높이, 그 외는 0
              {
                float t = 1 - (dx / warpHalfPx); // 중앙에서 가장 높고, 가장자리로 갈수록 낮아짐
                h = Mathf.Sin(t*Mathf.PI * 0.5f) * settings.crimpStrength; // 사인 곡선을 이용하여 중앙에서 부드럽게 높아졌다가 가장자리로 갈수록 빠르게 낮아짐. crimp 가 클수록 더 평평해짐.
              }
              else h = 0f;              
            } 
            else
            {
              if (dy <= weftHalfPx) // 셀 중앙에서 weftHalfPx 이내는 위사 높이, 그 외는 0
              {
                float t = 1 - (dy / weftHalfPx); // 중앙에서 가장 높고, 가장자리로 갈수록 낮아짐
                h = Mathf.Sin(t*Mathf.PI * 0.5f) * settings.crimpStrength; // 사인 곡선을 이용하여 중앙에서 부드럽게 높아졌다가 가장자리로 갈수록 빠르게 낮아짐. crimp 가 클수록 더 평평해짐.
              }
              else h = 0f;
            }

            int texX = cx * pixelsPerWarp + px;
            int texY = height - 1 - (cy * pixelsPerWeft + py);
            Color c = new(h, h, h);
            pixels[texY * width + texX] = c;

          }
        }
      }
    }
    dest.SetPixels32(pixels);
    dest.Apply(true);
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
          (byte)(r * 255),       // R
          (byte)(r * 255),       // G
          (byte)(r * 255),       // B
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
