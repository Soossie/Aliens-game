using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public readonly List<LevelData> Levels = new();
    [SerializeField] private LevelConfig levelConfig;
    public GameObject[] alienPrefabs;
    public AudioClip[] music;
    public int currentScore;
    public int currentLevel;
    public float timeLimit = 600f;
    private float lastContinueTime;
    public string lastSelectedObject;
    
    public static float MusicVolume = 0.5f;
    public static float SfxVolume = 1f;
    
    public bool inLevel;
    public bool isPaused;
    private bool inLevelMenu;
    private bool inSettingsMenu;
    public bool inDropDown;
    private bool inQuitMenu;
    public bool selectedWithKeyboard;
    private bool newGame;
    private bool won;
    private bool allLemmingsSpawned;
    private bool gameOver;
    private bool timerRunning;
    [SerializeField]
    private int latestLevel;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI timeText;
    private PlayerInput input;
    private EventSystem eventSystem;
    private Slider sfxSlider;
    private Slider musicSlider;

    private const string PREF_WIDTH = "resolution_width";
    private const string PREF_HEIGHT = "resolution_height";
    private const string PREF_FULLMODE = "display_mode";
    private const string PREF_VSYNC = "resolution_vsync";
    private const string PREF_SFXVOLUME = "sfx_volume";
    private const string PREF_MUSICVOLUME = "music_volume";

    //Initialization
    void Awake()
    {
        if (!_instance)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadPrefs();
    }
    
    void Start()
    {
        /*
        Levels.Add(new LevelData
        {
            levelName = "Level 1",
            isCompleted = false,
            perfectScore = false,
            firstTimeInLevel = true,
            Assets = new[] { 
                "Graphics/background_lvl_1", 
                "Graphics/bitmap_lvl_1", 
                "Level 1 Song" 
            },
            requiredScore = 5,
            spawnPoint = new Vector3(-9.916667f, -3.222222f),
            goalPoint = new Vector3(8.166667f, -8.222222f),
            lemmingsAmount = 10,
            unlocks = new[] {"Floater", "Basher"}
        });
        Levels.Add(new LevelData
        {
            levelName = "Level 2",
            isCompleted = false,
            perfectScore = false,
            firstTimeInLevel = true,
            Assets = new[] { 
                "Graphics/background_lvl_2", 
                "Graphics/bitmap_lvl_2", 
                "Level 2 Song" 
            },
            requiredScore = 10,
            spawnPoint = new Vector3(-9.722222f, 0.25f),
            goalPoint = new Vector3(7.666667f, -0.8611111f),
            lemmingsAmount = 20,
            unlocks = new[] {"Floater", "Basher", "Blocker", "Builder"}
        });
        Levels.Add(new LevelData
        {
            levelName = "Level 3",
            isCompleted = false,
            perfectScore = false,
            firstTimeInLevel = true,
            Assets = new[] { 
                "Graphics/background_lvl_3", 
                "Graphics/bitmap_lvl_3", 
                "Level 3 Song" 
            },
            requiredScore = 15,
            spawnPoint = new Vector3(-14.05556f, -3.402778f),
            goalPoint = new Vector3(14.47222f, -2.263889f),
            lemmingsAmount = 25,
            unlocks = new[] {"Floater", "Basher", "Blocker", "Builder", "Climber", "Digger"}
        });
        
        Levels.Add(new LevelData
        {
            LevelName = "Level 4",
            IsCompleted = false,
            PerfectScore = false,
            FirstTimeInLevel = true,
            Assets = new[] { 
                "Graphics/background test 2w-2", 
                "Graphics/bitmap_lvl_1", 
                "Level 4 Song" 
            },
            RequiredScore = 20,
            SpawnPoint = new Vector3(0f, 0f),
            GoalPoint = new Vector3(0f, 0f),
            LemmingsAmount = 25,
            Unlocks = new[] {"Floater", "Basher", "Blocker", "Builder", "Climber", "Digger"}
        });
        */
        if (levelConfig)
            Levels.AddRange(levelConfig.levels);
        
        Load();
        SubscribeToInput();
        ControlsMenuSetup();
        
        sfxSlider = GameObject.Find("SFX Volume Slider").GetComponent<Slider>();
        musicSlider = GameObject.Find("Music Volume Slider").GetComponent<Slider>();
        sfxSlider.SetValueWithoutNotify(SfxVolume);
        musicSlider.SetValueWithoutNotify(MusicVolume);
    }
    

    void Update()
    {
        if (inLevel && !isPaused && timerRunning)
            Timer();
        
        if (allLemmingsSpawned && inLevel && !gameOver && !isPaused 
            && (GameObject.FindGameObjectsWithTag("Lemming").Length + currentScore < Levels[currentLevel].requiredScore 
                || GameObject.FindGameObjectsWithTag("Lemming").Length == 0 || timeLimit == 0))
        {
            Debug.Log("Game Over");
            if (eventSystem)
                StartCoroutine(LevelEnd());
        }                              
        //Debug.Log(lastSelectedObject);
        Cursor.visible = input.currentControlScheme == "Keyboard&Mouse";
    }
    
    //Scene management
    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1;
        isPaused = false;
        StopAllCoroutines();
        Debug.Log("Loading level: " + levelName);
        currentLevel = Levels.FindIndex(level => level.levelName == levelName);
        Debug.Log("Current level index: " + currentLevel);
        SceneManager.LoadScene(2);
        StartCoroutine(LevelStart("Level"));
    }
    
    private IEnumerator LevelStart(string sceneName)
    {
        MusicManager.StopMusic();
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Loading");
        SceneManager.LoadScene(1);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);
        input.actions.FindActionMap("In Menu").Disable();
        inLevel = true;
        won = false;
        allLemmingsSpawned = false;
        gameOver = false;
        timeLimit = 600f;
        currentScore = 0;
        SubscribeToInput();
        
        sfxSlider = GameObject.Find("SFX Volume Slider").GetComponent<Slider>();
        musicSlider = GameObject.Find("Music Volume Slider").GetComponent<Slider>();
        sfxSlider.SetValueWithoutNotify(SfxVolume);
        musicSlider.SetValueWithoutNotify(MusicVolume);
        
        scoreText = GameObject.Find("ScoreCounter").GetComponent<TextMeshProUGUI>();
        timeText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        scoreText.text = "Aliens required: " + currentScore + " / " + Levels[currentLevel].requiredScore;
        timeText.text = timeLimit.ToString("F0");

        if (newGame)
        {
            newGame = false;
            latestLevel = 0;
            foreach (var t in Levels)
            {
                t.firstTimeInLevel = true;
                t.perfectScore = false;
                t.isCompleted = false;
            }

            yield return StartCoroutine(StartCutscene());
        }
        else
        {
            GameObject.Find("Cutscene Canvas").GetComponent<Canvas>().enabled = false;
        }
        
        if (Levels[currentLevel].firstTimeInLevel)
        {
            Levels[currentLevel].firstTimeInLevel = false;
            Save();
            var buttonslist = new List<string>(Levels[currentLevel].unlocks) {"Kill"};
            buttonslist.Insert(0, "Kill");

            foreach (var t in Levels[currentLevel].unlocks)
            {
                AudioManager.PlaySound(SoundType.RoleUnlock);
                Color panelColor = GameObject.Find(t + " Panel").GetComponent<Image>().color;
                GameObject.Find(t + " Button").GetComponent<Button>().interactable = true;

                for (float i = panelColor.a; i > 0f; i -= 5f / 255f)
                {
                    panelColor.a -= 5f / 255f;
                    GameObject.Find(t + " Panel").GetComponent<Image>().color = panelColor;
                    yield return new WaitForSeconds(0.02f);
                }

                int index = buttonslist.IndexOf(t);
                var navigation = GameObject.Find(t + " Button").GetComponent<Button>().navigation;
                navigation.selectOnLeft = GameObject.Find(buttonslist[index - 1] + " Button").GetComponent<Button>();
                navigation.selectOnRight = GameObject.Find(buttonslist[index + 1] + " Button").GetComponent<Button>();
                GameObject.Find(t + " Button").GetComponent<Button>().navigation = navigation;
            }

            var killNav = GameObject.Find("Kill Button").GetComponent<Button>().navigation;
            killNav.selectOnRight = GameObject.Find(buttonslist[1] + " Button").GetComponent<Button>();
            killNav.selectOnLeft = GameObject.Find(buttonslist[^2] + " Button").GetComponent<Button>();
            GameObject.Find("Kill Button").GetComponent<Button>().navigation = killNav;
            Debug.Log("Kill button navigation: left = " + killNav.selectOnLeft.name + ", right = " + killNav.selectOnRight.name);
            foreach (var button in buttonslist)
                Debug.Log("Button: " + button);

            
            var normalNav = GameObject.Find("Normal Button").GetComponent<Button>().navigation;
            normalNav.selectOnLeft = GameObject.Find("Kill Button").GetComponent<Button>();
            normalNav.selectOnRight = GameObject.Find(buttonslist[2] + " Button").GetComponent<Button>();
            GameObject.Find("Normal Button").GetComponent<Button>().navigation = normalNav;

        }
        else
        {
            var buttonslist = new List<string>(Levels[currentLevel].unlocks) {"Kill"};
            buttonslist.Insert(0, "Kill");
            
            foreach (var t in Levels[currentLevel].unlocks)
            {
                Color panelColor = GameObject.Find(t + " Panel").GetComponent<Image>().color;
                panelColor.a = 0f;
                GameObject.Find(t + " Panel").GetComponent<Image>().color = panelColor;
                GameObject.Find(t + " Button").GetComponent<Button>().interactable = true;
                
                int index = buttonslist.IndexOf(t);
                var navigation = GameObject.Find(t + " Button").GetComponent<Button>().navigation;
                navigation.selectOnLeft = GameObject.Find(buttonslist[index - 1] + " Button").GetComponent<Button>();
                navigation.selectOnRight = GameObject.Find(buttonslist[index + 1] + " Button").GetComponent<Button>();
                GameObject.Find(t + " Button").GetComponent<Button>().navigation = navigation;
            }
            
            var killNav = GameObject.Find("Kill Button").GetComponent<Button>().navigation;
            killNav.selectOnRight = GameObject.Find(buttonslist[1] + " Button").GetComponent<Button>();
            killNav.selectOnLeft = GameObject.Find(buttonslist[^2] + " Button").GetComponent<Button>();
            GameObject.Find("Kill Button").GetComponent<Button>().navigation = killNav;
            
            var normalNav = GameObject.Find("Normal Button").GetComponent<Button>().navigation;
            normalNav.selectOnLeft = GameObject.Find("Kill Button").GetComponent<Button>();
            normalNav.selectOnRight = GameObject.Find(buttonslist[2] + " Button").GetComponent<Button>();
            GameObject.Find("Normal Button").GetComponent<Button>().navigation = normalNav;

        }
        ControlsLevelSetup();
        MusicManager.SetMusicVolume(MusicVolume);
        MusicManager.PlayMusic(Levels[currentLevel].levelMusic);
        
        GameObject normalAlien = Array.Find(alienPrefabs, a => a.name == "Normal");
        for (int i = 0; i < Levels[currentLevel].lemmingsAmount; i++)
        {
            if (!inLevel) break;
            Instantiate(normalAlien, Levels[currentLevel].spawnPoint, Quaternion.identity);
            //Debug.Log("Lemming Spawned, total: " + (i + 1));
            yield return new WaitForSeconds(1.5f); //1.5f
        }
        
        allLemmingsSpawned = true;
    }
    
    private IEnumerator MainMenuLoader()
    {
        AudioManager.PlaySound(SoundType.UIClickIn);
        Time.timeScale = 1;
        input.actions.FindActionMap("In Menu").Disable();
        input.actions.FindActionMap("In Level").Enable();
        eventSystem.GetComponent<InputSystemUIInputModule>().move = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Navigate"));
        eventSystem.GetComponent<InputSystemUIInputModule>().point = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Point"));
        eventSystem.GetComponent<InputSystemUIInputModule>().leftClick = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Select Lemming"));
        isPaused = false;
        Debug.Log("Leaving Pause Menu to Main Menu");

        SceneManager.LoadScene("MainMenu");
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu");
        AudioClip mainMenuTheme = Array.Find(music, clip => clip.name == "Main Menu Theme");
        MusicManager.PlayMusic(mainMenuTheme);
        timeLimit = 600f;
        currentScore = 0;
        input.actions.FindActionMap("In Level").Disable();
        input.actions.FindActionMap("In Menu").Enable();
        Save();
        ControlsMenuSetup();
        SubscribeToInput();
        
        sfxSlider = GameObject.Find("SFX Volume Slider").GetComponent<Slider>();
        musicSlider = GameObject.Find("Music Volume Slider").GetComponent<Slider>();
        sfxSlider.SetValueWithoutNotify(SfxVolume);
        musicSlider.SetValueWithoutNotify(MusicVolume);

        inLevel = false;
    }
    
    private IEnumerator LevelMenuLoader()
    {
        GameObject.Find("Levels Menu").GetComponent<Canvas>().enabled = true;
        yield return null;
        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
            eventSystem.SetSelectedGameObject(GameObject.Find("Level 1 Button"));
        else 
            lastSelectedObject = "Level 1 Button";
        inLevelMenu = true;
    }
    
    private IEnumerator SettingsMenuLoader()
    {
        AudioManager.PlaySound(SoundType.UIClickIn);
        GameObject.Find("Settings Menu").GetComponent<Canvas>().enabled = true;
        yield return null;
        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
        {
            eventSystem.SetSelectedGameObject(GameObject.Find("Display Modes"));
            lastSelectedObject = "Display Modes";
        }
        else
        {
            lastSelectedObject = "Display Modes";
            eventSystem.SetSelectedGameObject(null);
        }
        inSettingsMenu = true;
    }

    private IEnumerator PauseMenuLoader()
    {
        AudioManager.PlaySound(SoundType.UIPaused);
        MusicManager.PauseMusic();

        GameObject.Find("Pause Menu").GetComponent<Canvas>().enabled = true;
        yield return null;
        input.actions.FindActionMap("In Level").Disable();
        input.actions.FindActionMap("In Menu").Enable();
        eventSystem.GetComponent<InputSystemUIInputModule>().move = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Navigate"));
        eventSystem.GetComponent<InputSystemUIInputModule>().point = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Point"));
        eventSystem.GetComponent<InputSystemUIInputModule>().leftClick = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Click"));
        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
        {
            eventSystem.SetSelectedGameObject(GameObject.Find("Continue Button"));
            lastSelectedObject = "Continue Button";

        }
        else
        {
            lastSelectedObject = "Continue Button";
            eventSystem.SetSelectedGameObject(null);
        }
        isPaused = true;
        Time.timeScale = 0;
        //Debug.Log("Paused");
    }
    
    private IEnumerator MoveHandler()
    {
        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            if (!eventSystem.currentSelectedGameObject)
            {
                yield return new WaitForNextFrameUnit();
                eventSystem.SetSelectedGameObject(GameObject.Find(lastSelectedObject));
            }
            else
                lastSelectedObject = eventSystem.currentSelectedGameObject.name;
        }

        if (input.currentControlScheme == "Gamepad")
        {
            if (!eventSystem.currentSelectedGameObject)
            {
                yield return new WaitForNextFrameUnit();
                eventSystem.SetSelectedGameObject(GameObject.Find(lastSelectedObject));
            }
            else
                lastSelectedObject = eventSystem.currentSelectedGameObject.name;
        }
    }

    private void ContinueHandler()
    {
        switch (inLevel)
        {
            case true when inDropDown:
            {
                //Debug.Log("From dropdown");
                inDropDown = false;
                GameObject.Find("Resolutions").GetComponent<TMP_Dropdown>().Hide();
                GameObject.Find("Display Modes").GetComponent<TMP_Dropdown>().Hide();
                break;
            }
            case true when inSettingsMenu && !gameOver:
            {
                AudioManager.PlaySound(SoundType.UIClickOut);
                GameObject.Find("Settings Menu").GetComponent<Canvas>().enabled = false;
                if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                {
                    eventSystem.SetSelectedGameObject(GameObject.Find("Settings Button"));
                    lastSelectedObject = "Settings Button";
                }
                else 
                    lastSelectedObject = "Settings Button";
                inSettingsMenu = false;
                break;
            }
            case true when !inSettingsMenu && !gameOver && !inQuitMenu:
            {
                AudioManager.PlaySound(SoundType.UIUnpaused);
                MusicManager.ResumeMusic();
                GameObject.Find("Pause Menu").GetComponent<Canvas>().enabled = false;
                Time.timeScale = 1;
                input.actions.FindActionMap("In Menu").Disable();
                input.actions.FindActionMap("In Level").Enable();
                eventSystem.GetComponent<InputSystemUIInputModule>().move = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Navigate"));
                eventSystem.GetComponent<InputSystemUIInputModule>().point = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Point"));
                eventSystem.GetComponent<InputSystemUIInputModule>().leftClick = InputActionReference.Create(input.actions.FindActionMap("In Level").FindAction("Select Lemming"));
                isPaused = false;
                if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                {
                    eventSystem.SetSelectedGameObject(GameObject.Find("Floater Button"));
                    lastSelectedObject = "Floater Button";
                }
                else 
                    lastSelectedObject = "Floater Button";

                break;
            }
            case true when inQuitMenu:
            {
                AudioManager.PlaySound(SoundType.UIClickOut);
                GameObject.Find("Quit Menu").GetComponent<Canvas>().enabled = false;
                inQuitMenu = false;
                if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                {
                    eventSystem.SetSelectedGameObject(GameObject.Find("Exit Button"));
                    lastSelectedObject = "Exit Button";
                }
                else
                {
                    lastSelectedObject = "Exit Button";
                    eventSystem.SetSelectedGameObject(null);
                }

                break;
            }
            default:
            {
                switch (inLevel)
                {
                    case false when inDropDown:
                    {
                        inDropDown = false;
                        GameObject.Find("Resolutions").GetComponent<TMP_Dropdown>().Hide();
                        GameObject.Find("Display Modes").GetComponent<TMP_Dropdown>().Hide();
                        break;
                    }
                    case false when inLevelMenu:
                    {
                        AudioManager.PlaySound(SoundType.UIClickOut);
                        GameObject.Find("Levels Menu").GetComponent<Canvas>().enabled = false;
                        inLevelMenu = false;
                        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                        {
                            eventSystem.SetSelectedGameObject(GameObject.Find("Levels"));
                            lastSelectedObject = "Levels";
                        }
                        else
                        {
                            lastSelectedObject = "Levels";
                            eventSystem.SetSelectedGameObject(null);
                        }

                        break;
                    }
                    case false when inSettingsMenu:
                    {
                        AudioManager.PlaySound(SoundType.UIClickOut);
                        GameObject.Find("Settings Menu").GetComponent<Canvas>().enabled = false;
                        inSettingsMenu = false;
                        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                        {
                            eventSystem.SetSelectedGameObject(GameObject.Find("Settings"));
                            lastSelectedObject = "Settings";
                        }
                        else
                        {
                            lastSelectedObject = "Settings";
                            eventSystem.SetSelectedGameObject(null);
                        }

                        break;
                    }
                    case false when inQuitMenu:
                    {
                        AudioManager.PlaySound(SoundType.UIClickOut);
                        GameObject.Find("Quit Menu").GetComponent<Canvas>().enabled = false;
                        inQuitMenu = false;
                        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
                        {
                            eventSystem.SetSelectedGameObject(GameObject.Find("Quit"));
                            lastSelectedObject = "Quit";
                        }
                        else
                        {
                            lastSelectedObject = "Quit";
                            eventSystem.SetSelectedGameObject(null);
                        }

                        break;
                    }
                    default:
                        Debug.Log("Redundant call");
                        break;
                }

                break;
            }
        }
    }
    
    //Button inputs
    public void QuitPopupLink()
    {
        AudioManager.PlaySound(SoundType.Quit);
        StartCoroutine(QuitPopup());
    }
    private IEnumerator QuitPopup()
    {
        GameObject.Find("Quit Menu").GetComponent<Canvas>().enabled = true;
        yield return null;
        if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")
        {
            eventSystem.SetSelectedGameObject(GameObject.Find("Stay Button"));
            lastSelectedObject = "Stay Button";
        }
        else
        {
            lastSelectedObject = "Stay Button";
            eventSystem.SetSelectedGameObject(null);
        }
        inQuitMenu = true;
    }

    public void Quit()
    {
        Save();
        Application.Quit();
    }
    
    public void LoadFirstLevel()
    {
        newGame = true;
        if (File.Exists(Application.persistentDataPath + "/saveData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/saveData.dat");
            Debug.Log("Save file deleted.");
        }
        Save();
        Load();
        LoadLevel("Level 1");
        AudioManager.PlaySound(SoundType.UINewGame);
    }

    public void LoadLatestLevel()
    {
        LoadLevel("Level " + (latestLevel + 1));
        AudioManager.PlaySound(SoundType.UINewGame);
    }

    private void LoadCurrentLevel()
    {
        timeLimit = 600f;
        StartCoroutine(LoadingCurrentLevel());
    }

    private IEnumerator LoadingCurrentLevel()
    {
        yield return null;
        LoadLevel("Level " + (currentLevel + 1));
    }

    private void LoadNextLevel()
    {
        AudioManager.PlaySound(SoundType.UINewGame);
        LoadLevel("Level " + (latestLevel + 1));
    }

    public void ToLevelMenu()
    {
        StartCoroutine(LevelMenuLoader());
    }

    public void FromLevelMenu()
    {
        ContinueHandler();
    }

    public void ToSettingsMenu()
    {
        StartCoroutine(SettingsMenuLoader());
    }

    public void FromSettingsMenu()
    {
        ContinueHandler();
    }

    public void FromQuitMenu()
    {
        ContinueHandler();
    }

    private void ToMainMenu()
    {
        StopAllCoroutines();
        AudioManager.PlaySound(SoundType.UIClickIn);
        StartCoroutine(MainMenuLoader());
    }
    
    
    //Input actions
    public void OnMove()
    {
        StartCoroutine(MoveHandler());
    }
    
    public void OnPoint()
    {
        if (input.currentControlScheme == "Keyboard&Mouse" && eventSystem.currentSelectedGameObject != null)
        {
            lastSelectedObject = eventSystem.currentSelectedGameObject.name;
            eventSystem.SetSelectedGameObject(null);
        }
    }
    
    public void OnClick(InputAction.CallbackContext context)
    {
        if (input.currentControlScheme == "Keyboard&Mouse" && eventSystem.currentSelectedGameObject != null && context.control.device is not Keyboard)
        {
            lastSelectedObject = eventSystem.currentSelectedGameObject.name;
            eventSystem.SetSelectedGameObject(null);
            selectedWithKeyboard = false;
        }
        else if (input.currentControlScheme == "Keyboard&Mouse" 
                 && eventSystem.currentSelectedGameObject != null
                 && context.control.device is Keyboard
                 && context.ReadValue<float>() < 0.5f)
            selectedWithKeyboard = true;
        /*if (lastSelectedObject is "Resolutions" or "Display Modes" && context.performed)
        {
            Debug.Log("Dropdown opened");
            inDropDown = true;
        }*/
        
    }
    
    public void OnPause(InputAction.CallbackContext context)
    {
        if (inLevel && context.performed)
        {
            StartCoroutine(PauseMenuLoader());
        }
    }
    
    public void OnContinue(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ContinueHandler();
        }
    }
    
    public void ChangeVolume(float volume, bool isSfx)
    {
        if (isSfx)
        {
            SfxVolume = volume;
            GameObject.Find("Audio Manager").GetComponent<AudioSource>().volume = volume;
            SaveSFXVolume(SfxVolume);
            return;
        }
    
        MusicVolume = volume;
        MusicManager.SetMusicVolume(MusicVolume);
        SaveMusicVolume(MusicVolume);
    }
    
    private void ControlsMenuSetup()
    {
        input.actions.FindActionMap("In Level").Disable();
        input.actions.FindActionMap("In Menu").Enable();
        if (File.Exists(Application.persistentDataPath + "/saveData.dat"))
        {
            GameObject.Find("Continue").GetComponent<Button>().interactable = true;
        }
        
        
        for (int i = 1; i < Levels.Count; i++)
        {
            if (Levels[i].isCompleted)
            {
                GameObject.Find("Level " + (i + 1) + " Panel").GetComponent<Image>().enabled = false;
                continue;
            }
            if (i == latestLevel) continue;
            GameObject.Find("Level " + (i + 1) + " Button").GetComponent<Button>().enabled = false;
            GameObject.Find("Level " + (i + 1) + " Panel").GetComponent<Image>().enabled = true;
        }
        
        for (int i = 0; i < Levels.Count; i++)
        {
            if (Levels[i].perfectScore) continue;
            GameObject.Find("Level " + (i + 1) + " Star").GetComponent<Image>().enabled = false;
            GameObject.Find("Levels Title").GetComponent<TextMeshProUGUI>().colorGradient = new VertexGradient(Color.white, Color.white, Color.white, Color.white);
            GameObject.Find("Levels Title").GetComponent<TextMeshProUGUI>().color = Color.lightGreen;
        }
        
        lastSelectedObject = "New Game";
        timerRunning = false;
    }

    private void ControlsLevelSetup()
    {
        input.actions.FindActionMap("In Level").Enable();
        lastSelectedObject = "Floater Button";
        
        if (input.currentControlScheme == "Keyboard&Mouse")
        {
            if (inLevel) 
                GameObject.Find("CursorVisual").GetComponent<Image>().enabled = false;
            
            if (selectedWithKeyboard)
                eventSystem.SetSelectedGameObject(GameObject.Find(lastSelectedObject));
            else
                eventSystem.SetSelectedGameObject(null);
        }
        
        else
        {
            if (inLevel)
                GameObject.Find("CursorVisual").GetComponent<Image>().enabled = true;
            eventSystem.SetSelectedGameObject(GameObject.Find(lastSelectedObject));
        }
        timerRunning = true;
    }

    private void OnControlsChanged(PlayerInput pi)
    { 
        if (pi.currentControlScheme == "Keyboard&Mouse")
        {
            if (inLevel)
                GameObject.Find("CursorVisual").GetComponent<Image>().enabled = false;
            if (eventSystem.currentSelectedGameObject)
                lastSelectedObject = eventSystem.currentSelectedGameObject.name;
        }
        else
        {
            if (inLevel)
                GameObject.Find("CursorVisual").GetComponent<Image>().enabled = true;
        }
        
        StartCoroutine(MoveHandler());
    }

    private void SubscribeToInput()
    {
        input = GetComponent<PlayerInput>();
        eventSystem = EventSystem.current;
        input.controlsChangedEvent.RemoveListener(OnControlsChanged);
        input.controlsChangedEvent.AddListener(OnControlsChanged);
        
        if (inLevel)
        {
            var continueObj = GameObject.Find("Continue Button");
            if (continueObj)
            {
                var continueButton = continueObj.GetComponent<Button>();
                continueButton.onClick.RemoveListener(ContinueHandler);
                continueButton.onClick.AddListener(ContinueHandler);
            }
            
            var settingsObj = GameObject.Find("Settings Button");
            if (settingsObj)
            {
                var settingsButton = settingsObj.GetComponent<Button>();
                settingsButton.onClick.RemoveListener(ToSettingsMenu);
                settingsButton.onClick.AddListener(ToSettingsMenu);
            }
            
            var fromSettingsObj = GameObject.Find("BackFromSettings");
            if (fromSettingsObj)
            {
                var fromSettingsButton = fromSettingsObj.GetComponent<Button>();
                fromSettingsButton.onClick.RemoveListener(ContinueHandler);
                fromSettingsButton.onClick.AddListener(ContinueHandler);
            }
            
            var exitPopupObj = GameObject.Find("Exit Button");
            if (exitPopupObj)
            {
                var exitButton = exitPopupObj.GetComponent<Button>();
                exitButton.onClick.RemoveListener(QuitPopupLink);
                exitButton.onClick.AddListener(QuitPopupLink); 
            }
            
            var exitStayObj = GameObject.Find("Stay Button");
            if (exitStayObj)
            {
                var exitStayButton = exitStayObj.GetComponent<Button>();
                exitStayButton.onClick.RemoveListener(ContinueHandler);
                exitStayButton.onClick.AddListener(ContinueHandler);
            }

            var quitObj = GameObject.Find("Quit Button");
            if (quitObj)
            {
                var quitButton = quitObj.GetComponent<Button>();
                quitButton.onClick.RemoveListener(ToMainMenu);
                quitButton.onClick.AddListener(ToMainMenu);
            }

            var victoryExitObj = GameObject.Find("Victory Exit Button");
            if (victoryExitObj)
            {
                var victoryExitButton = victoryExitObj.GetComponent<Button>();
                victoryExitButton.onClick.RemoveListener(ToMainMenu);
                victoryExitButton.onClick.AddListener(ToMainMenu);
            }
            
            var defeatExitObj = GameObject.Find("Defeat Exit Button");
            if (defeatExitObj)
            {
                var defeatExitButton = defeatExitObj.GetComponent<Button>();
                defeatExitButton.onClick.RemoveListener(ToMainMenu);
                defeatExitButton.onClick.AddListener(ToMainMenu);
            }
            
            var nextLevelObj = GameObject.Find("Next Level Button");
            if (nextLevelObj)
            {
                var nextLevelButton = nextLevelObj.GetComponent<Button>();
                nextLevelButton.onClick.RemoveListener(LoadNextLevel);
                nextLevelButton.onClick.AddListener(LoadNextLevel);
            }
            
            var retryObj = GameObject.Find("Retry Button");
            if (retryObj)
            {
                var retryButton = retryObj.GetComponent<Button>();
                retryButton.onClick.RemoveListener(LoadCurrentLevel);
                retryButton.onClick.AddListener(LoadCurrentLevel);
            }
            
            var retryObj2 = GameObject.Find("Retry Button 2");
            if (retryObj2)
            {
                var retryButton = retryObj2.GetComponent<Button>();
                retryButton.onClick.RemoveListener(LoadCurrentLevel);
                retryButton.onClick.AddListener(LoadCurrentLevel);
            }
        }
    }
    
    
    //In level
    public void ScoreCounter() 
    {
        currentScore++;
        if (currentScore >= Levels[currentLevel].requiredScore)
            Win();
        scoreText.text = "Aliens required: " + currentScore + " / " + Levels[currentLevel].requiredScore;
    }

    private void Win()
    {
        if (won) return;
        Debug.Log("Win");
        AudioManager.PlaySound(SoundType.ReachScore);
        won = true;
        timeText.color = Color.darkGreen;
        scoreText.color = Color.darkGreen;
    }

    private IEnumerator LevelEnd() 
    { 
        isPaused = true;                                                                                                                                           
        Time.timeScale = 0;
        gameOver = true;
        MusicManager.StopMusic();
        input.actions.FindActionMap("In Level").Disable();                                                                                                       
        input.actions.FindActionMap("In Menu").Enable();                                                                                                         
        eventSystem.GetComponent<InputSystemUIInputModule>().move = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Navigate"));  
        eventSystem.GetComponent<InputSystemUIInputModule>().point = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Point"));    
        eventSystem.GetComponent<InputSystemUIInputModule>().leftClick = InputActionReference.Create(input.actions.FindActionMap("In Menu").FindAction("Click"));
        yield return null;                                                                                                                                                          
        float percent = (float)currentScore / Levels[currentLevel].lemmingsAmount * 100f;
        
        if (won)
        {
            if (currentLevel == latestLevel) latestLevel++;
            Levels[currentLevel].isCompleted = true;
            won = false;
            Save();
            GameObject.Find("Victory Score Text").GetComponent<TextMeshProUGUI>().text = "Aliens saved: " + percent + "%";
            GameObject.Find("Victory Canvas").GetComponent<Canvas>().enabled = true;    
            if (currentScore == Levels[currentLevel].lemmingsAmount)
            {
                GameObject.Find("Victory Score Text").GetComponent<TextMeshProUGUI>().text += "\nPerfect Score!";
                Levels[currentLevel].perfectScore = true;
            }
            
            if (currentLevel == 2)
            {
                AudioManager.PlaySound(SoundType.GameWin);

                var nextLevelObj = GameObject.Find("Next Level Button");
                if (nextLevelObj)
                {
                    nextLevelObj.GetComponent<Button>().interactable = false;
                    GameObject.Find("Victory Exit Button").GetComponent<Button>().interactable = false;
                    yield return new WaitForSecondsRealtime(8f);
                    nextLevelObj.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
                    var nextLevelButton = nextLevelObj.GetComponent<Button>();
                    nextLevelButton.onClick.RemoveListener(LoadNextLevel);
                    nextLevelButton.onClick.AddListener(BeginEndCutscene);
                    nextLevelObj.GetComponent<Button>().interactable = true;
                    GameObject.Find("Victory Exit Button").GetComponent<Button>().interactable = true;
                }
                
                GameObject.Find("Victory Canvas").GetComponent<Canvas>().enabled = true;    
                if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")                      
                {                                                                                         
                    eventSystem.SetSelectedGameObject(GameObject.Find("Victory Exit Button"));                
                    lastSelectedObject = "Victory Exit Button";                                               
                }                                                                                         
                else                                                                                      
                {                                                                                         
                    lastSelectedObject = "Victory Exit Button";                                               
                    eventSystem.SetSelectedGameObject(null);                                              
                }
                yield break;
            }
            
            AudioManager.PlaySound(SoundType.LevelWin);

            if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")                      
            {                                                                                         
                eventSystem.SetSelectedGameObject(GameObject.Find("Next Level Button"));                
                lastSelectedObject = "Next Level Button";                                               
            }                                                                                         
            else                                                                                      
            {                                                                                         
                lastSelectedObject = "Next Level Button";                                               
                eventSystem.SetSelectedGameObject(null);                                              
            }  
            
            yield return new WaitForSecondsRealtime(5.5f);
            AudioClip victoryTheme = Array.Find(music, clip => clip.name == "Victory Theme");
            MusicManager.PlayMusic(victoryTheme);
        }
        
        else
        {
            AudioManager.PlaySound(SoundType.LevelFail);
            GameObject.Find("Defeat Score Text").GetComponent<TextMeshProUGUI>().text = "Aliens saved: " + percent + "%";
            GameObject.Find("Defeat Canvas").GetComponent<Canvas>().enabled = true;      
            
            if (selectedWithKeyboard || input.currentControlScheme == "Gamepad")                      
            {                                                                                         
                eventSystem.SetSelectedGameObject(GameObject.Find("Retry Button"));                
                lastSelectedObject = "Retry Button";                                               
            }                           
            
            else                                                                                      
            {                                                                                         
                lastSelectedObject = "Retry Button";                                               
                eventSystem.SetSelectedGameObject(null);                                              
            }

            yield return new WaitForSecondsRealtime(3f);
            AudioClip defeatTheme = Array.Find(music, clip => clip.name == "Defeat Theme");
            MusicManager.PlayMusic(defeatTheme);
        }
    }

    private void Timer()
    {
        if (timeLimit <= 0f)
        {
            timeLimit = 0f;
            timerRunning = false;
            return;
        }
        timeLimit -= Time.deltaTime;
        timeText.text = "Timer: " + timeLimit.ToString("F0");
    }
    
    private IEnumerator StartCutscene()
    {
        AudioClip levelStartTheme = Array.Find(music, clip => clip.name == "Level Start Music");
        MusicManager.PlayMusic(levelStartTheme);
        float volume = MusicVolume;
        yield return new WaitForSeconds(2f);
        Debug.Log("Cutscene started");

        string cutsceneText = "On an ordinary summer day...";
        TextMeshProUGUI cutsceneTextField = GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>();
        Color cutsceneTextFieldColor = cutsceneTextField.color;
        Color cutscenePanel = GameObject.Find("Cutscene Panel").GetComponent<Image>().color;
        Color visibleColor = cutsceneTextFieldColor;
        cutsceneTextField.maxVisibleCharacters = 0;
        yield return new WaitForNextFrameUnit();
        cutsceneTextField.text = cutsceneText;
        
        foreach (char unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 70; i++)
        {
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i > 20)
            {
                cutsceneTextFieldColor.a -= 0.05f;
                GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = cutsceneTextFieldColor; 
            }
        }
        
        cutsceneTextField.maxVisibleCharacters = 0;
        GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = visibleColor;
        cutsceneTextFieldColor = visibleColor;
        cutsceneText = "An alien ship crashes";
        cutsceneTextField.text = cutsceneText;
        
        foreach (var unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (var i = 0; i < 70; i++)
        {
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i > 20)
            {
                cutsceneTextFieldColor.a -= 0.05f;
                GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = cutsceneTextFieldColor; 
            }
        }

        for (var i = 0; i < 50; i++)
        {
            cutscenePanel.a -= 0.02f;
            GameObject.Find("Cutscene Panel").GetComponent<Image>().color = cutscenePanel; 
            yield return new WaitForSeconds(0.05f);
            MusicManager.SetMusicVolume(volume -= 0.02f);
        }
        GameObject.Find("Cutscene Canvas").GetComponent<Canvas>().enabled = false;
        Debug.Log("Cutscene finished");
    }

    private IEnumerator EndCutscene()
    {
        GameObject.Find("Cutscene Canvas").GetComponent<Canvas>().enabled = true;
        AudioClip levelEndTheme = Array.Find(music, clip => clip.name == "Level End Music");
        MusicManager.PlayMusic(levelEndTheme);
        yield return new WaitForSeconds(2f);
        Debug.Log("Cutscene started");
        
        string cutsceneText = "After a journey across the earth,";
        TextMeshProUGUI cutsceneTextField = GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>();
        Color cutsceneTextFieldColor = cutsceneTextField.color;
        Color visibleColor = cutsceneTextFieldColor;
        cutsceneTextField.maxVisibleCharacters = 0;
        yield return new WaitForNextFrameUnit();
        cutsceneTextField.text = cutsceneText;
        Color logoColor = GameObject.Find("Cutscene Logo").GetComponent<Image>().color;
        
        foreach (char unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 70; i++)
        {
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i > 20)
            {
                cutsceneTextFieldColor.a -= 0.05f;
                GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = cutsceneTextFieldColor; 
            }
        }
        
        cutsceneTextField.maxVisibleCharacters = 0;
        GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = visibleColor;
        cutsceneTextFieldColor = visibleColor;
        cutsceneText = "Where friends were lost and found.";
        cutsceneTextField.text = cutsceneText;
        
        foreach (var unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (var i = 0; i < 70; i++)
        {
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i > 20)
            {
                cutsceneTextFieldColor.a -= 0.05f;
                GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = cutsceneTextFieldColor; 
            }
        }
        
        cutsceneTextField.maxVisibleCharacters = 0;
        GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = visibleColor;
        cutsceneTextFieldColor = visibleColor;
        cutsceneText = "The Aliens were finally able to return home safely.";
        cutsceneTextField.text = cutsceneText;
        
        foreach (var unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (var i = 0; i < 70; i++)
        {
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i > 20)
            {
                cutsceneTextFieldColor.a -= 0.05f;
                GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = cutsceneTextFieldColor; 
            }
        }

        cutsceneTextField.maxVisibleCharacters = 0;
        GameObject.Find("Cutscene Text").GetComponent<TextMeshProUGUI>().color = visibleColor;
        cutsceneText = "Thank you for playing.";
        cutsceneTextField.text = cutsceneText;
        
        foreach (var unused in cutsceneTextField.text)
        {
            cutsceneTextField.maxVisibleCharacters++;
            cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        
        for (var i = 0; i < 70; i++)
        {
            if (i < 20)
                cutsceneTextField.characterSpacing += 0.1f;
            yield return new WaitForSeconds(0.05f);
            if (i <= 20) continue;
            cutsceneTextField.transform.position -= new Vector3(0f, 6.5f, 0f);
            GameObject.Find("Cutscene Text").transform.position = cutsceneTextField.transform.position;
        }
        
        for (var i = 0; i < 100; i++)
        {
            logoColor.a += 0.02f;
            GameObject.Find("Cutscene Logo").GetComponent<Image>().color = logoColor;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(50f);
        GameObject.Find("Cutscene Canvas").GetComponent<Canvas>().enabled = false;
        Debug.Log("Cutscene finished");
    }

    private void BeginEndCutscene()
    {
        Save();
        GameObject.Find("Victory Canvas").GetComponent<Canvas>().enabled = false;    
        StartCoroutine(EndCutscene());
        var nextLevelObj = GameObject.Find("Next Level Button");
        var nextLevelButton = nextLevelObj.GetComponent<Button>();
        nextLevelButton.onClick.RemoveListener(BeginEndCutscene);
        Time.timeScale = 1;
    }
    
    public void ChangeLemming(GameObject lemming, Vector2 lemmingPosition, string lemmingType)
    {
        var oldLemmingMoveDir = lemming.GetComponent<LemmingBase>().moveDir; //get old movedir and apply to new one
        var oldLemmingLastDir = lemming.GetComponent<LemmingBase>().lastDir;

        oldLemmingMoveDir = oldLemmingMoveDir switch
        {
            < -1 => -1,
            > 1 => 1,
            _ => oldLemmingMoveDir
        };

        oldLemmingLastDir = oldLemmingLastDir switch
        {
            < -1 => -1,
            > 1 => 1,
            _ => oldLemmingLastDir
        };

        Destroy(lemming);
        if (lemmingType == "Kill") return;
        GameObject alienRolePrefab = Array.Find(alienPrefabs, a => a.name == lemmingType);
        GameObject newLemming = Instantiate(alienRolePrefab, lemmingPosition, Quaternion.identity);
        newLemming.GetComponent<LemmingBase>().moveDir = oldLemmingMoveDir;
        newLemming.GetComponent<LemmingBase>().lastDir = oldLemmingLastDir;
    }

    
    //Save & Load system
    private void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/saveData.dat");
        
        SaveData data = new SaveData
        {
            latestLevel = Math.Max(currentLevel, latestLevel),
            levelSaves = new List<LevelSaveInfo>(),
        };

        foreach (var lvl in Levels)
        {
            data.levelSaves.Add(new LevelSaveInfo
            {
                levelName = lvl.levelName,
                isCompleted = lvl.isCompleted,
                perfectScore = lvl.perfectScore,
                firstTimeInLevel = lvl.firstTimeInLevel
            });
        }
        
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Saved");
    }

    private void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/saveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath+"/saveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();
            latestLevel = data.latestLevel;
            foreach (var lvlData in data.levelSaves)
            {
                var level = Levels.Find(lvl => lvl.levelName == lvlData.levelName);
                if (level != null)
                {
                    level.isCompleted = lvlData.isCompleted;
                    level.perfectScore = lvlData.perfectScore;
                    level.firstTimeInLevel = lvlData.firstTimeInLevel;
                }
            }
            //Debug.Log("Loaded");
        }
    }

    [Serializable]
    class LevelSaveInfo
    {
        public string levelName;
        public bool isCompleted;
        public bool perfectScore;
        public bool firstTimeInLevel;
    }

    [Serializable]
    class SaveData
    {
        public List<LevelSaveInfo> levelSaves;
        public int latestLevel;
    }
    
    

    public void SaveDisplayMode(int mode)
    {
        PlayerPrefs.SetInt(PREF_FULLMODE, mode);
    }

    public void SaveResolution(Resolution r)
    {
        PlayerPrefs.SetInt(PREF_WIDTH, r.width);
        PlayerPrefs.SetInt(PREF_HEIGHT, r.height);
    }

    public void SaveVsync(int v)
    {
        PlayerPrefs.SetInt(PREF_VSYNC, v);
    }

    private void SaveSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_SFXVOLUME, volume);
    }

    private void SaveMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_MUSICVOLUME, volume);
    }

    public void LoadPrefs()
    {
        MusicVolume = PlayerPrefs.GetFloat(PREF_MUSICVOLUME);
        SfxVolume = PlayerPrefs.GetFloat(PREF_SFXVOLUME);
        
        
        FullScreenMode mode = PlayerPrefs.GetInt(PREF_FULLMODE) switch
        {
            // Exclusive full screen is windows exclusive, fall back to fullscreenwindow on macos & linux
            0 => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor
                ? FullScreenMode.ExclusiveFullScreen
                : FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => Screen.fullScreenMode
        };
        
        Screen.SetResolution(
            PlayerPrefs.GetInt(PREF_WIDTH),
            PlayerPrefs.GetInt(PREF_HEIGHT),
            mode
        );
        QualitySettings.vSyncCount = PlayerPrefs.GetInt(PREF_VSYNC);
    }
}
