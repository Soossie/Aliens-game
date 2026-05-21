using System.Collections;
using UnityEngine;                                                                                                                                 
                                                                                                                                                   
public class Digger : LemmingBase                                                                                                                 
{                                 
    private static readonly int Digging = Animator.StringToHash("Digging");

    protected override void Start()
    {
        moveDir *= 2;
        base.Start();
    }          
    
    protected override void HandleMovement()
    {
        base.HandleMovement();
        if (moveDir is 2 or -2)
            StartCoroutine(Dig());
    }
                                                                                                                                                   
    private IEnumerator Dig()                                                                                                                               
    {                                                                                                                                              
        if (!TerrainCollision.IsWalkable(BelowPos) ||
            !TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) ||
            !TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            lastDir = moveDir / 2;
            moveDir = 0;
            SelfTimeScale = 1;
            animator.SetBool(Digging, false);
            yield break;
        }
        SelfTimeScale = 8;
        animator.SetBool(Digging, true);
        animator.SetBool(WalkRight, false);
        animator.SetBool(Falling, false);
        for (int i = 0; i < 8; i++)
        {
            Vector2 digPos = RightPos + new Vector2(-1f / Ppu - i * (1f / Ppu), -1f / Ppu);
            if (TerrainCollision.IsWalkable(digPos))
            {
                if (!TerrainCollision.IsDestroyable(digPos))
                {
                    moveDir /= 2;
                    SelfTimeScale = 1;
                    animator.SetBool(Digging, false);
                    yield break;
                }
                TerrainCollision.ChangeColor(digPos, Color.white, Color.black);
                yield return null;
            }
            else break;
        }
        transform.position = (Vector2)transform.position + new Vector2(0f, -1f / Ppu);
    }      
    
}                                                                                                                                                  