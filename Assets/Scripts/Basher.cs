using System.Collections;
using UnityEngine;

public class Basher : LemmingBase
{
    private static readonly int Bashing = Animator.StringToHash("Bashing");
    private int counter;

    protected override void HandleBlocked(int fallbackDir)
    {
        if (fallbackDir == 0)
        {
            moveDir = 0;
            return;
        }
        StartCoroutine(Bash());
    }

    public override void MoveRight()
    {
        SelfTimeScale = 1;
        counter++;
        if (counter > 1)
            Animator.SetBool(Bashing, false);
        base.MoveRight();
    }
    
    public override void MoveLeft()
    {
        SelfTimeScale = 1;
        counter++;
        if (counter > 1)
            Animator.SetBool(Bashing, false);
        base.MoveLeft();
    }

    public override void MoveDown()
    {
        SelfTimeScale = 1;
        Animator.SetBool(Bashing, false);
        base.MoveDown();
    }

    private IEnumerator Bash()
    {
        counter = 0;
        Animator.SetBool(Bashing, true);
        Animator.SetBool(WalkRight, false);
        SelfTimeScale = 3;
        switch (moveDir)
        {
            case 1:
                for (int i = 0; i <= 15; i++)
                {
                    Vector2 bashPos = RightPos + new Vector2(0, i * (1f / Ppu));
                    if (TerrainCollision.IsWalkable(bashPos))
                    {
                        if (!TerrainCollision.IsDestroyable(bashPos))
                        {
                            moveDir = -1;
                            SelfTimeScale = 1;
                            break;
                        }
                        TerrainCollision.ChangeColor(bashPos, Color.white, Color.black);
                        yield return null;
                    }
                }
                break;
            case -1:  
                for (int i = 0; i <= 14; i++)
                {
                    Vector2 bashPos = LeftPos + new Vector2(0, i * (1f / Ppu));
                    if (TerrainCollision.IsWalkable(bashPos))
                    {
                        if (!TerrainCollision.IsDestroyable(bashPos) && TerrainCollision.IsWalkable(bashPos))
                        {
                            moveDir = 1;
                            SelfTimeScale = 1;
                            break;
                        }
                        TerrainCollision.ChangeColor(bashPos, Color.white, Color.black);
                        yield return null;
                    }
                }
                break;
        }
    }
}
