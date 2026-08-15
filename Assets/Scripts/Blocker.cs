using System.Collections;
using UnityEngine;

public class Blocker : LemmingBase
{

    private bool beNormal;
    private static readonly int Blocking = Animator.StringToHash("Blocking");
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        if (!TerrainCollision.IsWalkable(BelowPos) && 
            !TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) &&
            !TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            beNormal = true;
            moveDir = 1;
        }
        else
            StartCoroutine(BlockPath());
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        if (beNormal)
            base.FixedUpdate();
        if (TerrainCollision.IsWalkable(BelowPos) ||
            TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) ||
            TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
            beNormal = false;
    }
    
    private IEnumerator BlockPath()
    {
        Animator.SetBool(Blocking, true);
        for (int i = 0; i < 16; i++)
        {
            Vector2 blockedPosRight = RightPos + new Vector2(1f / Ppu, i * (1f / Ppu));
            Vector2 blockedPosLeft = LeftPos + new Vector2(-1f / Ppu, i * (1f / Ppu));
            TerrainCollision.ChangeColor(blockedPosLeft, new Color(0.000f, 1.000f, 1));
            TerrainCollision.ChangeColor(blockedPosRight, new Color(0.000f, 1.000f, 1));
            yield return null;
        }

        for (int i = 0; i < 8; i++)
        {
            TerrainCollision.ChangeColor(LeftPos + new Vector2(i * (1f / Ppu), 15f / Ppu), 
                new Color(0.000f, 1.000f, 1));
            yield return null;
        }
    }

    void OnDestroy()
    {
        if (beNormal || !TerrainCollision)
            return;
        for (int i = 0; i < 16; i++)
        {
            Vector2 blockedPosRight = RightPos + new Vector2(1f / Ppu, i * (1f / Ppu));
            Vector2 blockedPosLeft = LeftPos + new Vector2(-1f / Ppu, i * (1f / Ppu));
            TerrainCollision.ChangeColor(blockedPosLeft, Color.white);
            TerrainCollision.ChangeColor(blockedPosRight, Color.white);
        }

        for (int i = 0; i < 8; i++)
        {
            TerrainCollision.ChangeColor(LeftPos + new Vector2(i * (1f / Ppu), 15f / Ppu), Color.white);
        }
    }
}
