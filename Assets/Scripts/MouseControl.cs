using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MouseControl : MonoBehaviour
{
    private VirtualMouseInput input;
    private PlayerInput playerInput;
    private InputAction clickAction;
    private InputAction navigateAction;
    private InputAction selectWithNumbersAction;
    private GameManager gameManager;
    private EventSystem eventSystem;
    private Image cursorImage;
    private string selectedRole;
    private GameObject selectedLemming;
    private Vector2 selectedLemmingPosition;
    [SerializeField] private Texture2D highlightedCursorTexture;
    
    private void Awake()
    {
        cursorImage = GameObject.Find("CursorVisual").GetComponent<Image>();
        input = GetComponent<VirtualMouseInput>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        playerInput = gameManager.GetComponent<PlayerInput>();                                              
        eventSystem = EventSystem.current;                                                                  
        
        clickAction = playerInput.actions.FindActionMap("In Level").FindAction("Select Lemming");           
        navigateAction = playerInput.actions.FindActionMap("In Level").FindAction("Navigate");              
        selectWithNumbersAction = playerInput.actions.FindActionMap("In Level").FindAction("Select Roles"); 
                                                                                                            
        clickAction.performed += OnClickPerformed;                                                            
        navigateAction.performed += OnNavigationPerformed;                                                    
        selectWithNumbersAction.performed += OnSelectWithNumbersPerformed;                                 
    }

    private void OnDisable()
    {
        clickAction.performed -= OnClickPerformed;
        navigateAction.performed -= OnNavigationPerformed;
        selectWithNumbersAction.performed -= OnSelectWithNumbersPerformed;
    }

    private void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        OnClick();
    }
    
    private void OnNavigationPerformed(InputAction.CallbackContext ctx)
    {
        OnNavigation();
    }

    private void OnSelectWithNumbersPerformed(InputAction.CallbackContext ctx)
    {
        SelectWithNumbers(ctx);
    }

    void Update()
    {
        cursorImage.rectTransform.localScale =
            new Vector3(Screen.height / 1080f, Screen.width / 1920f, 1f);
        if (gameManager.isPaused)
            GetComponent<VirtualMouseInput>().cursorSpeed = 0;
        else
            GetComponent<VirtualMouseInput>().cursorSpeed = 700 * Screen.width / 1920f;

    }
    
    private void LateUpdate()
    {
            Vector2 virtualMousePosition = input.virtualMouse.position.value;
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 300f * Screen.width / 1920f, Screen.width - 300f * Screen.width / 1920f);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 200f * Screen.height / 1080f, Screen.height - 200f * Screen.height / 1080f);
            InputState.Change(input.virtualMouse.position, virtualMousePosition);
            LemmingsCursor();
    }
    
    public void OnClick()
    {
        if (selectedLemming != null && selectedRole != null)
        {
            if (selectedRole == "Kill")
                selectedLemming.GetComponent<LemmingBase>().Die();
            else
                gameManager.ChangeLemming(selectedLemming, selectedLemmingPosition, selectedRole );
            AudioManager.PlaySound(SoundType.SelectLemming);
        }
    }
    
    private void LemmingsCursor()
    {
        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Vector2 screen = Mouse.current.position.ReadValue();
            if (screen.x < 300f || screen.x > Screen.width - 300f || screen.y < 200f || screen.y > Screen.height - 200f)
                return;
            Vector3 worldPos =
                Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Camera.main.nearClipPlane));

            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Lemming").Length; i++)
            {
                GameObject lemming = GameObject.FindGameObjectsWithTag("Lemming")[i];
                if (Vector2.Distance(worldPos, lemming.transform.position) < 0.4f)
                {
                    Cursor.SetCursor(highlightedCursorTexture, Vector2.zero, CursorMode.Auto);
                    //Debug.Log(lemming.name + " Neaby!!!");
                    selectedLemming = GameObject.FindGameObjectsWithTag("Lemming")[i];
                    selectedLemmingPosition = selectedLemming.transform.position;
                    break;
                }
                
                Cursor.SetCursor(gameManager.cursorTexture, Vector2.zero, CursorMode.Auto);
                //Debug.Log("No Lemmings Nearby");
                selectedLemming = null;
            }
        }

        else
        {
            Vector2 screen = input.virtualMouse.position.value;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Camera.main.nearClipPlane));
            
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Lemming").Length; i++)      
            {                                                                                  
                GameObject lemming = GameObject.FindGameObjectsWithTag("Lemming")[i];          
                if (Vector2.Distance(worldPos, lemming.transform.position) < 0.4f)             
                {                                                                              
                    cursorImage.color = Color.red;
                    selectedLemming = GameObject.FindGameObjectsWithTag("Lemming")[i];
                    selectedLemmingPosition = selectedLemming.transform.position;
                    break;
                }
                
                cursorImage.color = Color.white;
                selectedLemming = null;
            }
        }
    }

    private void OnNavigation()
    {
        StartCoroutine(WaitAndThenSelect());
        //Debug.Log("navigated");
    }

    private void SelectWithNumbers(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.control.name);
        
        switch (ctx.control.name)
        {
            case "1":
                SelectedButton("Normal Button");
                break;
            case "2":
                SelectedButton("Floater Button");
                break;
            case "3":
                SelectedButton("Basher Button");
                break;
            case "4":
                if (gameManager.Levels[gameManager.currentLevel].Unlocks.Contains("Blocker"))
                    SelectedButton("Blocker Button");
                break;
            case "5":
                if (gameManager.Levels[gameManager.currentLevel].Unlocks.Contains("Builder"))
                    SelectedButton("Builder Button");
                break;
            case "6":
                if (gameManager.Levels[gameManager.currentLevel].Unlocks.Contains("Climber"))
                    SelectedButton("Climber Button");
                break;
            case "7":
                if (gameManager.Levels[gameManager.currentLevel].Unlocks.Contains("Digger"))
                    SelectedButton("Digger Button");
                break;
            case "8":
                SelectedButton("Kill Button");
                break;
        }
        
    }

    private IEnumerator WaitAndThenSelect()
    {
        yield return new WaitForNextFrameUnit();
        if (eventSystem.currentSelectedGameObject)
            SelectedButton(eventSystem.currentSelectedGameObject.name); 
    }
    public void SelectedButton(string buttonName)
    {
        eventSystem.SetSelectedGameObject(GameObject.Find(buttonName));
        selectedRole = buttonName.Substring(0, buttonName.Length - " Button".Length);
        //Debug.Log(buttonName + " selected.");
    }
}
