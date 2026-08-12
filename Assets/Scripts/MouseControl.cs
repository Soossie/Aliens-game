using System;
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
    private GameObject selectedAlien;
    private Vector2 selectedAlienPosition;
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
        if (ctx.ReadValue<float>() > 0.5f)
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
        if (selectedAlien != null && selectedRole != null)
        {
            if (selectedRole == "Kill")
                selectedAlien.GetComponent<LemmingBase>().Die();
            else
            {
                gameManager.ChangeLemming(selectedAlien, selectedAlienPosition, selectedRole);
            }
            AudioManager.PlaySound(SoundType.SelectLemming);
        }
    }
    
    private void LemmingsCursor()
    {
        GameObject hoveredAlien = null;
        
        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Vector2 screen = Mouse.current.position.ReadValue();
            if (screen.x < 300f || screen.x > Screen.width - 300f || screen.y < 200f || screen.y > Screen.height - 200f)
                return;
            Vector3 worldPos =
                Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Camera.main.nearClipPlane));


            foreach (GameObject alien in GameObject.FindGameObjectsWithTag("Lemming"))
            {
                if (Mathf.Abs(worldPos.x - alien.transform.position.x) < 0.12f
                    && worldPos.y - alien.transform.position.y < 0.55f 
                    && worldPos.y - alien.transform.position.y > -0.03f) 
                {
                    hoveredAlien = alien;
                    break;
                }
            }

            if (hoveredAlien != selectedAlien)
            {
                if (selectedAlien != null)
                    selectedAlien.GetComponent<LemmingBase>().highlighted = false;
                
                if (hoveredAlien != null)
                    hoveredAlien.GetComponent<LemmingBase>().highlighted = true;
                
                selectedAlien = hoveredAlien;
            }
        }

        else
        {
            Vector2 screen = input.virtualMouse.position.value;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Camera.main.nearClipPlane));
            
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Lemming").Length; i++)      
            {                                                                                  
                GameObject alien = GameObject.FindGameObjectsWithTag("Lemming")[i];          
                if (Mathf.Abs(worldPos.x - alien.transform.position.x) < 0.07f
                    && worldPos.y - alien.transform.position.y < 0.01f 
                    && worldPos.y - alien.transform.position.y > -0.2f)      
                {                                                                              
                    cursorImage.color = Color.red;
                    selectedAlien = GameObject.FindGameObjectsWithTag("Lemming")[i];
                    selectedAlienPosition = selectedAlien.transform.position;
                    break;
                }
                
                cursorImage.color = Color.white;
                selectedAlien = null;
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
