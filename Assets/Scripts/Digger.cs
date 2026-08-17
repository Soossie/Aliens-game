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
        SelfTimeScale = 8;
        Animator.SetBool(Digging, true);
        Animator.SetBool(WalkRight, false);
        Animator.SetBool(Falling, false);
        bool onlyAir = true;
        for (int i = 0; i < 8; i++)
        {
            Vector2 digPos = RightPos + new Vector2(-1f / Ppu - i * (1f / Ppu), -1f / Ppu);
            if (!TerrainCollision.IsWalkable(digPos)) continue;
            if (!TerrainCollision.IsDestroyable(digPos))
            {
                moveDir /= 2;
                SelfTimeScale = 1;
                Animator.SetBool(Digging, false);
                yield break;
            }
            TerrainCollision.ChangeColor(digPos, Color.white, Color.black);
            onlyAir = false;
            yield return null;
        }
        if (onlyAir)
        {
            moveDir = 0;
            SelfTimeScale = 1;
            Animator.SetBool(Digging, false);
            yield break;
        }
        transform.position = (Vector2)transform.position + new Vector2(0f, -1f / Ppu);
    }      
    
}                                                                                                                                                  