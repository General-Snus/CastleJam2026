using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, CastleInputActions.IPlayerActions
{
    public static PlayerController instance;
    public UIInventory uiInventory;
    public UIInputCassetMenu uiCassetMenu;

    public Casset currentlyHeldCasset;
    public const int playerInventorySize = 3;
    public float moveSpeed = 2;
    public float rotateSpeed = 10;

    [SerializeField] Camera camera;
    [SerializeField] float upperCameraRotation = 75;
    [SerializeField] float lowerCameraRotation = -75;
    [SerializeField] float lowerCameraFoW = 25;
    [SerializeField] float upperCameraFoW = 80;
    [SerializeField] Rigidbody rb;
    [SerializeField] CharacterController cc;
    [SerializeField] GameObject cameraLayout;

    public float interactDistance = 2;
    public float initialJumpvelocity = 2.5f;
    float startMovespeed = 10;
    float startFow = 10;
    CastleInputActions inputActions;
    CastleInputActions.PlayerActions playerActions;
    Vector3 moveDirection;
    bool captureCameraActive = false;


    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }

        instance = this;
        inputActions = new();
        playerActions = inputActions.Player;
        playerActions.SetCallbacks(this);
        startMovespeed = moveSpeed;
        startFow = camera.fieldOfView;
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        uiInventory.UpdateUI(this);
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }

    void OnEnable()
    {
        playerActions.Enable();
    }

    void OnDisable()
    {
        playerActions.Disable();
    }

    float gravity = 0;

    Interactable currentHover;
    void Update()
    {
        if (cc.isGrounded && gravity < 0) { gravity = 0; }
        gravity += Physics.gravity.y * Time.deltaTime;

        var movement = Vector3.zero;
        movement += moveSpeed * transform.forward * moveDirection.z;
        movement += moveSpeed * transform.right * moveDirection.x;
        movement.y = gravity;

        cc.Move(moveSpeed * movement * Time.deltaTime);


        var ray = camera.ViewportPointToRay(Vector3.one * .5f);
        var layerMask = 1 << LayerMask.NameToLayer("Interactable");
        if (!Physics.Raycast(
            ray,
            out var hitInfo,
            maxDistance: interactDistance,
            layerMask
            ))
        {
            uiCassetMenu.gameObject.SetActive(false);
            return;
        }

        if (hitInfo.collider.gameObject.TryGetComponent<TripodController>(out var tripod))
        {
            currentHover = tripod;
            uiCassetMenu.gameObject.SetActive(true);
        }

        if (hitInfo.collider.gameObject.TryGetComponent<CassetContainer>(out var cassetContainer))
        {
            uiCassetMenu.gameObject.SetActive(false);
            currentHover = tripod;
        }
    }

    public void DeactiveCamera()
    {
        camera.enabled = false;
    }

    public void ActiveCamera()
    {
        camera.enabled = true;
    }

    public void DeactiveControlls()
    {
        playerActions.Disable();
    }

    public void ActivateControlls()
    {
        playerActions.Enable();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Cursor.lockState = CursorLockMode.Locked;
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime * context.ReadValue<Vector2>().x, Space.World);

        var rotation = camera.transform.localEulerAngles;
        rotation.x += rotateSpeed * Time.deltaTime * -context.ReadValue<Vector2>().y;
        rotation.x = clampAngle(rotation.x, lowerCameraRotation, upperCameraRotation);
        camera.transform.localEulerAngles = rotation;

        float clampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        var ray = camera.ViewportPointToRay(Vector3.one * .5f);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 1);
        var layerMask = 1 << LayerMask.NameToLayer("Interactable");
        if (!Physics.Raycast(
            ray,
            out var hitInfo,
            maxDistance: interactDistance,
            layerMask
            ))
        {
            return;
        }

        if (hitInfo.collider.gameObject.TryGetComponent<TripodController>(out var tripod))
        {
            tripod.Interact(this);
        }

        if (hitInfo.collider.gameObject.TryGetComponent<CassetContainer>(out var cassetContainer))
        {
            cassetContainer.Interact(this);
            uiInventory.UpdateUI(this);
        }

        Debug.Log($"Interacted with {hitInfo.collider.gameObject}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        Debug.DrawRay(transform.position, Vector3.down * .5f, Color.red, 1);
        if (!cc.isGrounded) { return; }

        gravity = initialJumpvelocity;
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        moveSpeed = startMovespeed;
        if (!context.performed) { return; }
        moveSpeed = startMovespeed * 2;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var v2 = context.ReadValue<Vector2>();
        moveDirection.x = v2.x;
        moveDirection.z = v2.y;
    }

    public void OnTakePhotograph(InputAction.CallbackContext context)
    {
        if (!captureCameraActive) { return; }
        if (currentlyHeldCasset is ProjectionCasset asset)
        {
            asset.captureToProject = Capture.CaptureWithCamera(camera);
            return;
        }
    }

    public void OnActivateCamera(InputAction.CallbackContext context)
    {
        captureCameraActive = context.performed;
        cameraLayout.SetActive(captureCameraActive);
        if (!captureCameraActive)
        {
            camera.fieldOfView = startFow;
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        SceneQuery.instance.ReloadScene();
    }

    public void OnCassetSelection1(InputAction.CallbackContext context)
    {
        uiInventory.UpdateUI(this);
        if (currentHover == null) { return; }
        if (currentHover is TripodController tripod)
        {
            tripod.switchCassets(this, 0);
            uiCassetMenu.UpdateUI(this, tripod);
            uiInventory.UpdateUI(this);
        }

    }

    public void OnCassetSelection2(InputAction.CallbackContext context)
    {
        uiInventory.UpdateUI(this);
        if (currentHover == null) { return; }
        if (currentHover is TripodController tripod)
        {
            tripod.switchCassets(this, 1);
            uiCassetMenu.UpdateUI(this, tripod);
            uiInventory.UpdateUI(this);
        }
    }

    public void OnCassetSelection3(InputAction.CallbackContext context)
    {
        uiInventory.UpdateUI(this);
        if (currentHover == null) { return; }
        if (currentHover is TripodController tripod)
        {
            tripod.switchCassets(this, 2);
            uiCassetMenu.UpdateUI(this, tripod);
            uiInventory.UpdateUI(this);
        }
    }


    public void OnZoom(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        if (captureCameraActive)
        {
            camera.fieldOfView += context.ReadValue<Vector2>().y * Time.deltaTime * 25;
            camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, lowerCameraFoW, upperCameraFoW);
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.TryGetComponent<KillCollider>(out var _))
        {
            SceneQuery.instance.ReloadScene();
        }

        if (hit.gameObject.CompareTag("Goal"))
        {
            SceneQuery.instance.NextScene();
        }

    }
}
