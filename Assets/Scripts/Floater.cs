using UnityEngine;

public sealed class Floater : LemmingBase
{
    public override void MoveDown()
    { 
        if (TerrainCollision.IsWalkable(BelowPos) ||
            TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) ||
            TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            SelfTimeScale = 1;
            moveDir = lastDir;
            Animator.SetBool(Falling, false);
            return;
        }

        SelfTimeScale = 2;
        Animator.SetBool(Falling, true);
        Animator.SetBool(WalkRight, false);
        transform.position = TargetPos + new Vector2(0, -1f / Ppu); 
    }
}
