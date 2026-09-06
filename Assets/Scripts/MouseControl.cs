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

public sealed class MouseControl : MonoBehaviour
{
    [HideInInspector] public VirtualMouseInput input;
    private PlayerInput playerInput;
    private InputAction clickAction;
    private InputAction navigateAction;
    private InputAction selectWithNumbersAction;
    private GameManager gameManager;
    private EventSystem eventSystem;
    private string selectedRole;
    private GameObject selectedAlien;
    private Vector2 selectedAlienPosition;
    
    private void Awake()
    {
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
        else
            eventSystem.SetSelectedGameObject(GameObject.Find(selectedRole + " Button"));
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
        //cursorImage.rectTransform.localScale =
            //new Vector3(Screen.height / 1080f, Screen.width / 1920f, 1f);
        if (gameManager.isPaused)
            GetComponent<VirtualMouseInput>().cursorSpeed = 0;
        else
            GetComponent<VirtualMouseInput>().cursorSpeed = 1400f;
        Debug.Log(eventSystem.currentSelectedGameObject);

    }
    
    private void LateUpdate()
    {
            Vector2 virtualMousePosition = input.virtualMouse.position.value;
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 600f * Screen.width / 3840f, Screen.width - 600f * Screen.width / 3840f);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 450f * Screen.height / 2160f, Screen.height - 450f * Screen.height / 2160f);
            InputState.Change(input.virtualMouse.position, virtualMousePosition);
            if (Time.timeScale != 0)
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
                selectedAlienPosition = selectedAlien.transform.position;
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
                if (Mathf.Abs(worldPos.x - alien.transform.position.x) < 0.14f
                    && worldPos.y - alien.transform.position.y < 0.5f 
                    && worldPos.y - alien.transform.position.y > -0.03f) 
                {
                    hoveredAlien = alien;
                    break;
                }
            }

            if (hoveredAlien != selectedAlien)
            {
                if (selectedAlien != null)
                    selectedAlien.GetComponent<LemmingBase>().SetHighlighted(false);
                
                if (hoveredAlien != null)
                    hoveredAlien.GetComponent<LemmingBase>().SetHighlighted(true);
                
                selectedAlien = hoveredAlien;
            }
        }

        else
        {
            Vector2 screen = input.virtualMouse.position.value;
            Vector3 worldPos = 
                Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Camera.main.nearClipPlane));
            
            foreach (GameObject alien in GameObject.FindGameObjectsWithTag("Lemming"))
            {
                if (Mathf.Abs(worldPos.x - alien.transform.position.x) < 0.15f
                    && worldPos.y - alien.transform.position.y < 0.46f 
                    && worldPos.y - alien.transform.position.y > -0.03f) 
                {
                    hoveredAlien = alien;
                    break;
                }
            }

            if (hoveredAlien != selectedAlien)
            {
                if (selectedAlien != null)
                    selectedAlien.GetComponent<LemmingBase>().SetHighlighted(false);
                
                if (hoveredAlien != null)
                    hoveredAlien.GetComponent<LemmingBase>().SetHighlighted(true);
                
                selectedAlien = hoveredAlien;
            }
        }
    }

    private void OnNavigation()
    {
        StartCoroutine(WaitAndThenSelect());
        Debug.Log("navigated");
    }

    private void SelectWithNumbers(InputAction.CallbackContext ctx)
    {
        //Debug.Log(ctx.control.name);
        if (ctx.ReadValue<float>() < 0.5f)
            return;
        
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
                if (gameManager.Levels[gameManager.currentLevel].unlocks.Contains("Blocker"))
                    SelectedButton("Blocker Button");
                break;
            case "5":
                if (gameManager.Levels[gameManager.currentLevel].unlocks.Contains("Builder"))
                    SelectedButton("Builder Button");
                break;
            case "6":
                if (gameManager.Levels[gameManager.currentLevel].unlocks.Contains("Climber"))
                    SelectedButton("Climber Button");
                break;
            case "7":
                if (gameManager.Levels[gameManager.currentLevel].unlocks.Contains("Digger"))
                    SelectedButton("Digger Button");
                break;
            case "8":
                SelectedButton("Kill Button");
                break;
        }
        
    }

    private IEnumerator WaitAndThenSelect()
    {
        Debug.Log("wait and then select");
        yield return new WaitForNextFrameUnit();
        if (eventSystem.currentSelectedGameObject)
            SelectedButton(eventSystem.currentSelectedGameObject.name); 
    }
    public void SelectedButton(string buttonName)
    {
        Debug.Log("selected button: " + buttonName);
        eventSystem.SetSelectedGameObject(GameObject.Find(buttonName));
        var newRole = buttonName.Substring(0, buttonName.Length - " Button".Length);
        if (newRole != selectedRole)
            AudioManager.PlaySound(SoundType.SelectLemming);
        selectedRole = newRole;
        
    }
}
