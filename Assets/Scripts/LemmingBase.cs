using System.Collections;
using UnityEngine;

public abstract class LemmingBase : MonoBehaviour, ILemming
{
    private SpriteRenderer spriteRenderer;
    protected Animator Animator;
    public bool highlighted;

    private GameManager gameManager;
    protected TerrainCollision TerrainCollision;

    protected static readonly int WalkRight = Animator.StringToHash("Walk_Right");
    protected static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int Highlighted = Animator.StringToHash("Highlighted");
    public int lastDir;
    protected float Ppu;
    private int currentLevel;
    protected Vector2 TargetPos;
    protected Vector2 LeftPos, RightPos, BelowPos;
    protected int FallHeight;

    [SerializeField]
    public int moveDir = 1;
    
    [SerializeField]
    protected float time = 0.05f;

    private int selfTime;
    protected int SelfTimeScale;
    
    protected virtual void Start()
    {
        AudioManager.PlaySound(SoundType.SpawnLemming, transform.position);
        spriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        TerrainCollision = GameObject.Find("Runtime Bitmap").GetComponent<TerrainCollision>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        Ppu = spriteRenderer.sprite.pixelsPerUnit;
        currentLevel = gameManager.currentLevel;
        
        LeftPos = transform.GetChild(0).transform.position;
        RightPos = transform.GetChild(1).transform.position;
        BelowPos = transform.GetChild(2).transform.position;
    }

    protected virtual void FixedUpdate()
    {
        if (gameManager.currentLevel == 2 && (transform.position.y < -7.513874f 
                                              || transform.position.x < -21.69611f
                                              || transform.position.x > 18.3611f))
                Die();
        Time.fixedDeltaTime = time;
        selfTime++;
        
        if (selfTime < SelfTimeScale)
            return;
        selfTime = 0;
        
        TargetPos = (Vector2)transform.position + new Vector2(moveDir, 0) * 1f / Ppu;
        LeftPos = transform.GetChild(0).transform.position;
        RightPos = transform.GetChild(1).transform.position;
        BelowPos = transform.GetChild(2).transform.position;
        
        HandleMovement();

        spriteRenderer.flipX = moveDir < 0;
        
        if (Vector2.Distance(transform.position, gameManager.Levels[currentLevel].goalPoint) <= 2f / Ppu)
        {
            ReachGoal();
        }
        if (TerrainCollision.IsKillable(BelowPos))
        {
            Die();
        }
        for (int i = 0; i < 16; i++)
        {
            if (TerrainCollision.IsKillable(LeftPos + new Vector2(0, i / Ppu)) || TerrainCollision.IsKillable(RightPos + new Vector2(0, i / Ppu)))
            {
                Die();
            }
        }
    }
    
    public void SetHighlighted(bool value)
    {
        Debug.Log("Setting highlighted to " + value);
        highlighted = value;
        Animator.SetBool(Highlighted, value);
    }

    protected virtual void HandleMovement()
    {
        switch (moveDir)
        {
            case 1:
                MoveRight();
                Animator.SetBool(WalkRight, true);
                Animator.SetBool(Falling, false);
                time = 0.05f;
                break;
            case -1:
                MoveLeft();
                Animator.SetBool(WalkRight, true);
                Animator.SetBool(Falling, false);
                time = 0.05f;
                break;
            case 0:
                MoveDown();
                Animator.SetBool(WalkRight, false);
                Animator.SetBool(Falling, true);
                time = 0.05f;
                break;
        }
    }

    protected virtual void HandleBlocked(int fallbackDir)
    {
        moveDir = fallbackDir;
    }
    
    public virtual void MoveRight()
    {
        // Checks if path forward is blocked, in which case turns around
        for (int i = 15; i >= 1; i--)
            if (TerrainCollision.IsWalkable(RightPos + new Vector2(0, i * (1f / Ppu))))
            {
                HandleBlocked(-1);
                return;
            }
        
            // Check if can go down right
        if (!TerrainCollision.IsWalkable(LeftPos + new Vector2(2f / Ppu, -1f / Ppu)) 
            && !TerrainCollision.IsWalkable(RightPos) 
            && TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) 
            && TerrainCollision.IsWalkable(LeftPos + new Vector2(2f / Ppu, -2f / Ppu))) 
            transform.position = TargetPos + new Vector2(0, -1f / Ppu);
        
            // Check if can move right
        else if (TerrainCollision.IsWalkable(RightPos + new Vector2(0, -1f / Ppu))
                 && !TerrainCollision.IsWalkable(RightPos))
        transform.position = TargetPos;
        
            // Check if can move right from left check
        else if (TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) && !TerrainCollision.IsWalkable(RightPos))
            transform.position = TargetPos;
        
            // Check if slope at right
        else if (TerrainCollision.IsWalkable(RightPos) && !TerrainCollision.IsWalkable(RightPos + new Vector2(0, 1f / Ppu)) && TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
            transform.position = TargetPos + new Vector2(0, 1f / Ppu);
        
        else if (!TerrainCollision.IsWalkable(BelowPos) 
                 && !TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) && !TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            lastDir = moveDir;
            HandleBlocked(0);                                                                                                       
        }
        
        else if (!TerrainCollision.IsWalkable(RightPos - new Vector2(0, -1f / Ppu)))
            transform.position = TargetPos + new Vector2(0, -1f / Ppu);
        
        else HandleBlocked(-1);
    }
    
    public virtual void MoveLeft()
    {
        for (int i = 14; i >= 1; i--)
            if (TerrainCollision.IsWalkable(LeftPos + new Vector2(0 / Ppu, i / Ppu)))
            {
                HandleBlocked(1);
                return;
            }

            //check if can go down left
        if (!TerrainCollision.IsWalkable(RightPos + new Vector2(-2f / Ppu, -1f / Ppu)) && !TerrainCollision.IsWalkable(LeftPos) && TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)) && TerrainCollision.IsWalkable(RightPos + new Vector2(-2f / Ppu, -2f / Ppu)))
            transform.position = TargetPos + new Vector2(0, -1f / Ppu);
        
        else if (TerrainCollision.IsWalkable(LeftPos + new Vector2(0, -1f / Ppu)) && !TerrainCollision.IsWalkable(LeftPos))
            transform.position = TargetPos; 
            
            //move left from right check
        else if (TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)) && !TerrainCollision.IsWalkable(LeftPos))
            transform.position = TargetPos;
        
            //check if slope at left
        else if (TerrainCollision.IsWalkable(LeftPos) &&
                 !TerrainCollision.IsWalkable(LeftPos + new Vector2(0, 1f / Ppu)) && TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu))) 
            transform.position = TargetPos + new Vector2(0, 1f / Ppu);
        
        else if (!TerrainCollision.IsWalkable(BelowPos) 
                 && !TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) && !TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            lastDir = moveDir;
            HandleBlocked(0);                                                                                                       
        }
        
        else if (!TerrainCollision.IsWalkable(LeftPos - new Vector2(0, -1f / Ppu)))
            transform.position = TargetPos + new Vector2(0, -1f / Ppu);
        
        else HandleBlocked(1);
    }

    public virtual void MoveDown()
    {
        if (TerrainCollision.IsWalkable(BelowPos) ||
            TerrainCollision.IsWalkable(LeftPos + new Vector2(1f / Ppu, -1f / Ppu)) ||
            TerrainCollision.IsWalkable(RightPos + new Vector2(-1f / Ppu, -1f / Ppu)))
        {
            moveDir = lastDir;
            Animator.SetBool(Falling, false);
            if (FallHeight < 80)
                FallHeight = 0;
            else
                Die();
            return;
        }
        transform.position = TargetPos + new Vector2(0, -1f / Ppu);
        FallHeight++;
    }

    public virtual void Die()
    {
        //stuff
        StartCoroutine(DoDie());
    }

    private IEnumerator DoDie()
    {
        AudioManager.PlaySound(SoundType.LemmingDie, transform.position);
        yield return null;
        Destroy(gameObject);
    }

    public virtual void ReachGoal()
    {
        //stuff
        AudioManager.PlaySound(SoundType.ReachGoal, transform.position);
        gameManager.ScoreCounter();
        Debug.Log("Lemming reached goal!");
        Destroy(gameObject);
    }
}