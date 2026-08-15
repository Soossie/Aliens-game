using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonLinker : MonoBehaviour
{
    private GameManager gameManager;
    private const float Cooldown = 0.1f;
    private float currentCooldown;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    public void LoadFirstLevel()
    {
        gameManager.LoadFirstLevel();
    }

    public void LoadCurrentLevel()
    {
        gameManager.LoadLatestLevel();
    }

    public void ToLevelMenu()
    {
        gameManager.ToLevelMenu();
    }

    public void FromLevelMenu()
    {
        gameManager.FromLevelMenu();
    }
    
    public void ToSettingsMenu()
    {
        gameManager.ToSettingsMenu();
    }

    public void FromSettingsMenu()
    {
        gameManager.FromSettingsMenu();
    }
    
    public void QuitPopup()
    {
        gameManager.QuitPopupLink();
    }

    public void FromQuitMenu()
    {
        gameManager.FromQuitMenu();
    }

    public void Quit()
    {
        gameManager.Quit();
    }
    
    public void LoadLevel(string levelName)
    {
        gameManager.LoadLevel(levelName);
    }
    
    public void ChangeSfxVolume(float value)
    {
        float volume = value;
        if (!gameManager) return;
        gameManager.ChangeVolume(volume, true);
        if (currentCooldown <= 0)
        {
            AudioManager.PlaySound(SoundType.SelectLemming, Vector3.zero, volume);
            currentCooldown = Cooldown;
        }
    }

    public void ChangeMusicVolume(float value)
    {
        float volume = value;
        if (!gameManager) return;
        gameManager.ChangeVolume(volume, false);
        if (currentCooldown <= 0)
        {
            AudioManager.PlaySound(SoundType.SelectLemming, Vector3.zero, volume);
            currentCooldown = Cooldown;
        }
    }
}
