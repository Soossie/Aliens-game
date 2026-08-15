using System;
using Sirenix.Utilities;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

public class AlienHoverInfo : MonoBehaviour
{
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descriptionText;
    private PlayerInput input;
    private EventSystem eventSystem;
    private const float CooldownTime = 1f;
    private float cooldown;
    private GameManager gameManager;
    private Canvas canvas;
    [HideInInspector] public string currentAlienRole;
    [HideInInspector] public string selectedAlienRole;
    private InputAction infoAction;
    private VirtualMouseInput virtualMouseInput;
    public bool showingSelected;

    private void Awake()
    {
        titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        descriptionText = transform.GetChild(1).Find("Description").GetComponent<TextMeshProUGUI>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        input = gameManager.GetComponent<PlayerInput>();
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        canvas = GetComponentInParent<Canvas>();
        infoAction = input.actions.actionMaps[0].FindAction("Alien Info");
        infoAction.performed += OnInfoRequested;      
        infoAction.canceled += OnInfoRequested;
        virtualMouseInput = GameObject.Find("Cursor").GetComponent<VirtualMouseInput>();
    }

    private void Update()
    {
        if (input.currentControlScheme == "Keyboard&Mouse")
            transform.position = Mouse.current.position.ReadValue() + new Vector2(-300,20);
        else
            transform.position = virtualMouseInput.virtualMouse.position.value;
        
        if (!string.IsNullOrEmpty(currentAlienRole) && !canvas.enabled && cooldown == 0)
        {
            cooldown = CooldownTime; // Start the cooldown
        }
        else if (cooldown > 0) // Cooldown
            cooldown -= Time.deltaTime;
        else if (!string.IsNullOrEmpty(currentAlienRole)) // When cooldown is finished
        {
            ShowHoverInfo(currentAlienRole);
            cooldown = 0;
        }
    }

    public void ShowHoverInfo(string button)
    {
        switch (button)
        {
            case "Normal Button":
                titleText.text = "Basic Alien";
                descriptionText.text = "Basic alien. Can move and fall from short heights.";
                canvas.enabled = true;
                break;
            case "Floater Button":
                titleText.text = "Floater Alien";
                descriptionText.text = "Falls slower and takes no fall damage.";
                canvas.enabled = true;
                break;
            case "Basher Button":
                titleText.text = "Basher Alien";
                descriptionText.text = "Punches destroyable terrain in front of it.";
                canvas.enabled = true;
                break;
            case "Blocker Button":
                titleText.text = "Blocker Alien";
                descriptionText.text = "Prevents other aliens from moving past it.";
                canvas.enabled = true;
                break;
            case "Builder Button":
                titleText.text = "Builder Alien";
                descriptionText.text = "Builds until it runs out of tiles or if it hits its head.";
                canvas.enabled = true;
                break;
            case "Climber Button":
                titleText.text = "Climber Alien";
                descriptionText.text = "Climbs straight walls and blockers.";
                canvas.enabled = true;
                break;
            case "Digger Button":
                titleText.text = "Digger Alien";
                descriptionText.text = "Digs straight down until it falls or hits unbreakable terrain.";
                canvas.enabled = true;
                break;
            case "Kill Button":
                titleText.text = "Alien Annihilator";
                descriptionText.text = "Kills the alien instantly. \n(it will not feel pain)";
                canvas.enabled = true;
                break;
            // Add more cases for other aliens as needed
            default:
                titleText.text = "Nothing Selected";
                descriptionText.text = "Select an alien to view its information.";
                canvas.enabled = true;
                break;
        }
    }

    public void HideHoverInfo()
    {
        currentAlienRole = null;
        cooldown = 0;
        
        if (canvas.enabled)
            canvas.enabled = false;
    }
    
    private void OnInfoRequested(InputAction.CallbackContext context)
    {
        Debug.Log("Requested info for: " + selectedAlienRole);
        if (context.performed)
        {
            ShowHoverInfo(selectedAlienRole);
            showingSelected = true;
        }
        else if (context.canceled)
        {
            HideHoverInfo();
            showingSelected = false;
        }
    }

    private void OnDisable()
    {
        infoAction.performed -= OnInfoRequested;
        infoAction.canceled -= OnInfoRequested;
    }
}
