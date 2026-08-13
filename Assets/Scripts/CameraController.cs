using System;
using UnityEngine;
using UnityEngine.InputSystem;
using PixelPerfectCamera = UnityEngine.Rendering.Universal.PixelPerfectCamera;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private PixelPerfectCamera pixelPerfectCamera;
    
    private readonly float moveSpeed = 10f;
    private Vector2 inputMove;
    private float inputClick;
    
    private float zoom;
    private const float ZoomMultiplier = 1.5f;
    private int minZoom = 18;
    private const int MaxZoom = 500;
    private float zoomInput;
    private Vector3 min;
    private Vector3 max;

    private InputAction moveAction;
    private InputAction clickAction;
    private InputAction zoomAction;
    private PlayerInput playerInput;
    private GameManager gameManager;

    private void OnEnable()
    {
        playerInput = FindAnyObjectByType<GameManager>().GetComponent<PlayerInput>();
        gameManager = FindAnyObjectByType<GameManager>();
        moveAction = playerInput.actions.actionMaps[0].FindAction("Move Camera");
        clickAction = playerInput.actions.actionMaps[0].FindAction("Select Lemming");
        zoomAction = playerInput.actions.actionMaps[0].FindAction("Zoom Camera");
        
        moveAction.performed += OnMovePerformed;
        clickAction.performed += OnClickPerformed;
        zoomAction.performed += OnZoomPerformed;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        clickAction.performed -= OnClickPerformed;
        zoomAction.performed -= OnZoomPerformed;
    }
    
    void Start()
    {
        if (gameManager.currentLevel == 1)
            minZoom = 33;
        mainCamera = GetComponent<Camera>();
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        zoom = pixelPerfectCamera.assetsPPU;
        transform.position = new Vector3(gameManager.Levels[gameManager.currentLevel].spawnPoint.x, gameManager.Levels[gameManager.currentLevel].spawnPoint.y, -10f);
        SpriteRenderer sr = GameObject.Find("Runtime Bitmap").GetComponent<SpriteRenderer>();
        min = sr.bounds.min;
        max = sr.bounds.max;
        
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => inputMove = ctx.ReadValue<Vector2>();

    private void OnClickPerformed(InputAction.CallbackContext ctx) => inputClick = ctx.ReadValue<float>();

    private void OnZoomPerformed(InputAction.CallbackContext ctx) => zoomInput = ctx.ReadValue<float>();

    void LateUpdate()
    {
        Vector3 move = new Vector3(inputMove.x, inputMove.y, 0f) * (moveSpeed * Time.deltaTime / 1.5f / zoom * 25);
        mainCamera.transform.position = 
            Mathf.Clamp(mainCamera.transform.position.x + move.x, 
                min.x + mainCamera.orthographicSize * mainCamera.aspect, 
                max.x - mainCamera.orthographicSize * mainCamera.aspect) 
            * Vector3.right 
            + Mathf.Clamp(mainCamera.transform.position.y + move.y, 
                min.y + mainCamera.orthographicSize, 
                max.y - mainCamera.orthographicSize) * Vector3.up 
            + Vector3.forward * mainCamera.transform.position.z;
        
        zoom -= zoomInput * ZoomMultiplier;
        zoom = Mathf.Clamp(zoom, minZoom, MaxZoom);
        pixelPerfectCamera.assetsPPU = (int)zoom;
    }

    void OnDestroy()
    {
        moveAction.Disable();
        clickAction.Disable();
        zoomAction.Disable();
    }
}
