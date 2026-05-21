using UnityEngine;

public class TerrainCollision : MonoBehaviour
{
    public SpriteRenderer bitmapSpriteRenderer;
    public SpriteRenderer terrainSpriteRenderer;
    private readonly Color walkableColor = new (0.000f, 0.000f, 0.000f, 1);
    private readonly Color nonDestroyableColor = new (0.000f, 1.000f, 1);
    private readonly Color killableColor = new (1.000f, 0.000f, 0.000f);

    private Texture2D originalBitmapTexture;
    private Sprite originalBitmapSprite;
    private Texture2D runtimeBitmap;
    
    private Texture2D originalTerrainTexture;
    private Sprite originalTerrainSprite;
    private Texture2D runtimeTerrain;
    
    public GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        GenerateRuntimeTerrain(gameManager.currentLevel);
        GenerateRuntimeBitmap(gameManager.currentLevel);
    }
    
    /*
    void Update()
    {
        var mouse = Mouse.current;
        
        Vector2 screenPos = mouse.position.ReadValue();
        Vector2 worldPos3 = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 mousePos = worldPos3;
        
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Color pixelcolor = GetPixelColorFromMousePosition(mousePos);
            Debug.Log(pixelcolor + " At " + mousePos);
            if (IsWalkable(mousePos)) Debug.Log("Walkable");
        }

        if (mouse.leftButton.isPressed)
        {
            ChangeColor(mousePos, new Color(143f / 255f, 86f / 255f, 59f / 255f, 255f / 255f), Color.black);
        }

        if (mouse.rightButton.isPressed)
        {
            ChangeColor(mousePos, new Color(95f / 255f, 205f / 255f, 228f / 255f), Color.white);
        }
    }
    */
    

    public bool IsWalkable(Vector2 worldPos)
    {
        Color pixelcolor = GetPixelColorFromMousePosition(worldPos);
        if (ColorsAreSimilar(pixelcolor, walkableColor) || ColorsAreSimilar(pixelcolor, nonDestroyableColor))
        {
            return true;
        }
        return false;
    }

    public bool IsKillable(Vector2 belowPos)
    {
        Color pixelcolor = GetPixelColorFromMousePosition(belowPos);
        if (ColorsAreSimilar(pixelcolor, killableColor))
        {
            return true;
        }
        return false;
    }

    public bool IsDestroyable(Vector2 worldPos)
    {
        Color pixelcolor = GetPixelColorFromMousePosition(worldPos);
        if (ColorsAreSimilar(pixelcolor, nonDestroyableColor))
        {
            return false;
        }
        return true;
    }

    private Color GetPixelColorFromMousePosition(Vector2 worldPos)
    {
        Vector2 localPos = bitmapSpriteRenderer.transform.InverseTransformPoint(worldPos);

        Sprite sprite = bitmapSpriteRenderer.sprite;
        Texture2D texture = sprite.texture;
        
        Vector2 pivot = sprite.pivot;
        float ppu = sprite.pixelsPerUnit;

        Vector2 pixelPos = new Vector2(pivot.x + localPos.x * ppu, pivot.y + localPos.y * ppu);
        
        return texture.GetPixel((int)pixelPos.x, (int)pixelPos.y);
    }

    public void ChangeColor(Vector2 worldPos, Color bitmapColor, Color terrainColor = default)
    {
        Vector2 bitmapLocalPos = bitmapSpriteRenderer.transform.InverseTransformPoint(worldPos);
        Vector2 terrainLocalPos = terrainSpriteRenderer.transform.InverseTransformPoint(worldPos);
        
        Sprite bitmapSprite = bitmapSpriteRenderer.sprite;
        Sprite terrainSprite = terrainSpriteRenderer.sprite;

        Rect bitmapTexRect = bitmapSprite.textureRect;
        Rect terrainTexRect = terrainSprite.textureRect;
        
        float ppu = bitmapSprite.pixelsPerUnit;
        
        int bx = Mathf.FloorToInt(bitmapTexRect.x + bitmapSprite.pivot.x + bitmapLocalPos.x * ppu);
        int by = Mathf.FloorToInt(bitmapTexRect.y + bitmapSprite.pivot.y + bitmapLocalPos.y * ppu);
        int tx = Mathf.FloorToInt(terrainTexRect.x + terrainSprite.pivot.x + terrainLocalPos.x * ppu);
        int ty = Mathf.FloorToInt(terrainTexRect.y + terrainSprite.pivot.y + terrainLocalPos.y * ppu);
        
        if (terrainColor != default)
            runtimeTerrain.SetPixel(tx, ty, terrainColor);
        runtimeBitmap.SetPixel(bx, by, bitmapColor);
        
        runtimeTerrain.Apply();
        runtimeBitmap.Apply();
    }
    
    
    bool ColorsAreSimilar(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
    
    private void GenerateRuntimeBitmap(int currentLevel)
    {
        originalBitmapSprite = Resources.Load<Sprite>(gameManager.Levels[currentLevel].Assets[1]);
        originalBitmapTexture = originalBitmapSprite.texture;
        Color[] pixels = originalBitmapTexture.GetPixels();
        Vector2 normalizedPivot = new Vector2(
            originalBitmapSprite.pivot.x / originalBitmapSprite.rect.width,
            originalBitmapSprite.pivot.y / originalBitmapSprite.rect.height
            );

        runtimeBitmap = new Texture2D(originalBitmapTexture.width, originalBitmapTexture.height,
            originalBitmapTexture.format, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        runtimeBitmap.SetPixels(pixels);
        runtimeBitmap.name = "Runtime Bitmap";
        
        runtimeBitmap.Apply();
        bitmapSpriteRenderer.sprite = Sprite.Create(
            runtimeBitmap,
            originalBitmapSprite.rect,
            normalizedPivot,
            36f
        );
    }

    private void GenerateRuntimeTerrain(int currentLevel)
    {
        originalTerrainSprite = Resources.Load<Sprite>(gameManager.Levels[currentLevel].Assets[0]);
        originalTerrainTexture = originalTerrainSprite.texture;
        Color[] pixels = originalTerrainTexture.GetPixels();
        Vector2 normalizedPivot = new Vector2(
            originalTerrainSprite.pivot.x / originalTerrainSprite.rect.width,
            originalTerrainSprite.pivot.y / originalTerrainSprite.rect.height
            );

        runtimeTerrain = new Texture2D(originalTerrainTexture.width, originalTerrainTexture.height,
            originalTerrainTexture.format, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        runtimeTerrain.SetPixels(pixels);
        runtimeTerrain.name = "Runtime Terrain";

        runtimeTerrain.Apply();
        terrainSpriteRenderer.sprite = Sprite.Create(
            runtimeTerrain,
            originalTerrainSprite.rect,
            normalizedPivot,
            36f
        );
    }
}