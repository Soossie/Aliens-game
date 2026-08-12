using UnityEngine;
using UnityEngine.UIElements;

public class ButtonLinker : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
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
    }

    public void ChangeMusicVolume(float value)
    {
        float volume = value;
        if (!gameManager) return;
        gameManager.ChangeVolume(volume, false);
    }
    
    //settings menu functions
}
