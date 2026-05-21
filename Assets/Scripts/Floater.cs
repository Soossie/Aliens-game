using UnityEngine;

public class Floater : LemmingBase
{
    public override void MoveDown()
    { 
        if (TerrainCollision.IsWalkable(BelowPos) ||
            TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) ||
            TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            SelfTimeScale = 1;
            moveDir = lastDir;
            animator.SetBool(Falling, false);
            return;
        }

        SelfTimeScale = 2;
        animator.SetBool(Falling, true);
        animator.SetBool(WalkRight, false);
        transform.position = targetPos + new Vector2(0, -1f / Ppu); 
    }
}
