using System;
using UnityEngine;

public class FontRenderer
{
  private CellDrawer _drawer;
  private Font _font;
  private Color32[] _fontPixels;
  private int _fontTextureWidth;
  private int _fontTextureHeight;

  public FontRenderer(CellDrawer drawer, Font font)
  {
    _drawer = drawer;
    _font = font;
  }

  //--------------------------------------------------------------------------
  public void DrawCharacter(char character, int col, int row, Color32 color)
  {
    string text = character.ToString();
    int fontSize = _drawer.CellSize - 4;
    _font.RequestCharactersInTexture(text, fontSize, FontStyle.Normal);

    if (_font.GetCharacterInfo(character, out CharacterInfo info, fontSize))
    {
      BlitGlyph(col, row, info, color);
    }
  }

  //--------------------------------------------------------------------------
  public void DrawNumber(int number, int col, int row, Color32 color)
  {
    string text = number.ToString();
    //int cellSize = _drawer.CellSize
    int fontSize = _drawer.CellSize - 4;
    _font.RequestCharactersInTexture(text, fontSize, FontStyle.Normal);
    int totalWidth = 0;
    foreach (char c in text)
    {
      if (_font.GetCharacterInfo(c, out CharacterInfo info, fontSize))
      {
        totalWidth += info.advance;
      }
    }

    int cursorX = (_drawer.CellSize - totalWidth) / 2;
    foreach (char c in text)
    {
      if (_font.GetCharacterInfo(c, out CharacterInfo info, fontSize))
      {
        BlitGlyph(col, row, info, color, cursorX);
        cursorX += info.advance;
      }
    }
  }
  //--------------------------------------------------------------------------
  // 숫자 1~N까지 미리 준비하여 캐시에 저장 (반복적으로 그릴 때 성능 향상)
  public void PrepareNumbers(int from, int to)
  {
    string allChars = "";
    for (int i = from; i <= to; i++)
      allChars += i.ToString();

    int fontSize = _drawer.CellSize - 4;
    _font.RequestCharactersInTexture(allChars, fontSize, FontStyle.Normal);
    CacheFontCharacters();
  }
  //--------------------------------------------------------------------------
  private void CacheFontCharacters()
  {
    Texture fontMainTex = _font.material.mainTexture;

    RenderTexture rt = RenderTexture.GetTemporary(fontMainTex.width, fontMainTex.height);
    Graphics.Blit(fontMainTex, rt);

    RenderTexture previous = RenderTexture.active;
    RenderTexture.active = rt;

    // RGBA32 포맷 명시 및 밉맵 생성 끄기(false)로 정확도 향상
    Texture2D readable = new Texture2D(fontMainTex.width, fontMainTex.height, TextureFormat.RGBA32, false);
    readable.ReadPixels(new Rect(0, 0, fontMainTex.width, fontMainTex.height), 0, 0);
    readable.Apply();

    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(rt);

    _fontPixels = readable.GetPixels32();
    _fontTextureWidth = readable.width;
    _fontTextureHeight = readable.height;

    // [중요] 임시 생성한 텍스처를 파괴하여 메모리 누수 방지
    UnityEngine.Object.Destroy(readable);
  }

  //--------------------------------------------------------------------------
  private void BlitGlyph(int col, int row, CharacterInfo info, Color32 color, int xOffset = -1)
  {
    int cellSize = _drawer.CellSize;

    // UV Y축이 뒤집혀 있으므로 Min/Max 정리
    float uvLeft = Mathf.Min(info.uvBottomLeft.x, info.uvTopRight.x);
    float uvRight = Mathf.Max(info.uvBottomLeft.x, info.uvTopRight.x);
    float uvBottom = Mathf.Min(info.uvBottomLeft.y, info.uvTopRight.y);
    float uvTop = Mathf.Max(info.uvBottomLeft.y, info.uvTopRight.y);

    bool yFlipped = info.uvBottomLeft.y > info.uvTopRight.y;

    int srcXMin = Mathf.RoundToInt(uvLeft * _fontTextureWidth);
    int srcYMin = Mathf.RoundToInt(uvBottom * _fontTextureHeight);
    int srcXMax = Mathf.RoundToInt(uvRight * _fontTextureWidth);
    int srcYMax = Mathf.RoundToInt(uvTop * _fontTextureHeight);

    int pixelWidth = srcXMax - srcXMin;
    int pixelHeight = srcYMax - srcYMin;

    if (pixelWidth <= 0 || pixelHeight <= 0) return;

    int offsetX = (cellSize - pixelWidth) / 2;
    int offsetY = (cellSize - pixelHeight) / 2;
    
    int destX = xOffset >= 0
      ? col * _drawer.CellSize + xOffset         // 다자리 숫자: 외부 지정 위치
      : col * _drawer.CellSize + (cellSize - pixelWidth) / 2; // 단일 문자: 셀 중앙
    int destY = row * cellSize + offsetY;

    for (int y = 0; y < pixelHeight; y++)
    {
      for (int x = 0; x < pixelWidth; x++)
      {
        int srcX = srcXMin + x;
        int srcY = srcYMin + y;

        if (srcX < 0 || srcX >= _fontTextureWidth || srcY < 0 || srcY >= _fontTextureHeight)
          continue;

        Color32 pixelColor = _fontPixels[srcY * _fontTextureWidth + srcX];
        
        if (pixelColor.a < 12) continue;

        int finalY = yFlipped ? destY + (pixelHeight - 1 - y) : destY + y;
        _drawer.SetPixel(destX + x, finalY, color);
      }
    }
  }
  //-------------------------------------------------------------------------
}
