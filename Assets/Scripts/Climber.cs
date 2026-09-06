using UnityEngine;

public sealed class Climber : LemmingBase
{
    private static readonly int Climbing = Animator.StringToHash("Climbing");

    protected override void HandleMovement()
    {
        base.HandleMovement();
        if (moveDir is 2 or -2)
            Climb();
    }
    
    protected override void HandleBlocked(int fallbackDir)
    {
        if (fallbackDir == 0)
        {
            moveDir = 0;
            return;
        }
        Climb();
    }

    private void Climb()
    {
        Animator.SetBool(Climbing, true);
        Animator.SetBool(WalkRight, false);
        Animator.SetBool(Falling, false);

        if (moveDir != 2 && moveDir != -2)
        {
            lastDir = moveDir;
            moveDir = lastDir >= 0 ? 2 : -2;
        }
        SelfTimeScale = 2;
        
        
        switch (moveDir)
        {
            case 2:
                if (TerrainCollision.IsWalkable(RightPos + new Vector2(0, 1f / Ppu)) && 
                       !TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, 15f / Ppu)))
                {
                    transform.position = (Vector2)transform.position + new Vector2(0f, 1f / Ppu);
                }
                else
                {
                    SelfTimeScale = 1;
                    Animator.SetBool(Climbing, false);
                    if (!TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, 15f / Ppu)))
                    {
                        transform.position = (Vector2)transform.position + new Vector2(1f / Ppu, 1f / Ppu);
                        moveDir = lastDir;
                        Animator.SetBool(Climbing, false);
                    }
                    else
                    {
                        lastDir = -1;
                        HandleBlocked(0);
                    }
                }
                break;
            
            case -2:  
                if (TerrainCollision.IsWalkable(LeftPos + new Vector2(0, 1f / Ppu)) && 
                    !TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, 15f / Ppu)))
                {
                    transform.position = (Vector2)transform.position + new Vector2(0f, 1f / Ppu);
                }
                else
                {
                    SelfTimeScale = 1;
                    Animator.SetBool(Climbing, false);
                    if (!TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, 15f / Ppu)))
                    {
                        transform.position = (Vector2)transform.position + new Vector2(-1f / Ppu, 1f / Ppu);
                        moveDir = lastDir;
                    }
                    else
                    {
                        lastDir = 1;
                        HandleBlocked(0);
                    }
                }
                break;
        }
    }
}
