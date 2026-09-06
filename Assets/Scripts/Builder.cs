using TMPro;
using UnityEngine;

public sealed class Builder : LemmingBase
{
    private  int tiles = 120;
    [SerializeField] private TextMeshProUGUI tileCounter;
    private int backToNormal;
    private static readonly int Building = Animator.StringToHash("Build");
    protected override void Start()
    {
        moveDir *= 2;
        base.Start();
        if (moveDir == 0)
            backToNormal = 3;
        tileCounter.text = tiles.ToString();
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();
        if (backToNormal > 3)
        {
            Animator.SetBool(Building, false);
            return;
        }
        if (moveDir is 2 or -2)
            Build();
    }
    
    
    protected override void HandleBlocked(int fallbackDir)
    {
        if (tiles != 0 && backToNormal < 3)
        {
            moveDir = fallbackDir * 2;
            Build();
        }
        else
        {
            Animator.SetBool(Building, false);
            moveDir = fallbackDir;
        }
    }

    void Build()
    {
        Animator.SetBool(Building, true);
        Animator.SetBool(WalkRight, false);
        Animator.SetBool(Falling, false);
        if (tiles == 0)
        {
            // after building turns into a normal alien
            moveDir /= 2;
            SelfTimeScale = 1;
            backToNormal = 3;
            return;
        }
        SelfTimeScale = 5;
        switch (moveDir)
        {
            case 2:
                bool isWalkableRight1 = TerrainCollision.IsWalkable(RightPos);
                bool isWalkableRight2 = TerrainCollision.IsWalkable(RightPos);
                bool headBlockedRight = TerrainCollision.IsWalkable(RightPos + new Vector2(0, 16f / Ppu));
                if (headBlockedRight)
                {
                    moveDir /= 2;
                    SelfTimeScale = 1;
                    backToNormal = 3;
                    Debug.Log("Head Blocked Right");
                }
                else if (backToNormal >= 3)
                {
                    moveDir /= 2;
                    SelfTimeScale = 1;
                }
                else if (!isWalkableRight1 && !isWalkableRight2)
                {
                    TerrainCollision.ChangeColor((RightPos + new Vector2(-1f / Ppu, -1f / Ppu)), Color.black, new Color(150f / 255f, 111f / 255f, 51f / 255f));
                    TerrainCollision.ChangeColor((RightPos + new Vector2(0, -1f / Ppu)), Color.black, new Color(150f / 255f, 111f / 255f, 51f / 255f));
                    transform.position = (Vector2)transform.position + new Vector2(1f / Ppu, 1f / Ppu);
                    tiles--;
                    tileCounter.text = tiles.ToString();
                }
                else
                {
                    moveDir = -2;
                    backToNormal++;
                    SelfTimeScale = 1;
                }
                break;
            case -2:
                bool isWalkableLeft1 = TerrainCollision.IsWalkable(LeftPos);
                bool isWalkableLeft2 = TerrainCollision.IsWalkable(LeftPos + new Vector2(-1f / Ppu, 0));
                bool headBlockedLeft = TerrainCollision.IsWalkable(LeftPos + new Vector2(0, 16f / Ppu));
                if (headBlockedLeft)
                {
                    moveDir /= 2;
                    backToNormal = 3;
                    SelfTimeScale = 1;
                    Debug.Log("Head Blocked Left");
                }
                else if (backToNormal >= 3)
                {
                    moveDir /= 2;
                    SelfTimeScale = 1;
                }
                else if (!isWalkableLeft1 && !isWalkableLeft2)
                {
                    TerrainCollision.ChangeColor(transform.position, Color.black, new Color(150f / 255f, 111f / 255f, 51f / 255f));
                    TerrainCollision.ChangeColor((Vector2)transform.position + new Vector2(-1f / Ppu, 0), Color.black, new Color(150f / 255f, 111f / 255f, 51f / 255f));
                    transform.position = (Vector2)transform.position + new Vector2(-1f / Ppu, 1f / Ppu);
                    tiles--;
                    tileCounter.text = tiles.ToString();
                }
                else
                {
                    moveDir = 2;
                    backToNormal++;
                    SelfTimeScale = 1;
                }
                break;
        }
    }
}
