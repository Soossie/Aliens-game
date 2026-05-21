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
    
    public void ToSettingsMenu()
    {
        gameManager.ToSettingsMenu();
    }

    public void Quit()
    {
        gameManager.Quit();
    }
    
    public void LoadLevel(string levelName)
    {
        gameManager.LoadLevel(levelName);
    }
    
    public void ChangeVolume(bool isSfx )
    {
        float volume = gameObject.GetComponent<Slider>().value;
        gameManager.ChangeVolume(volume, isSfx);
    }
    
    //settings menu functions
}
