using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{
    void Interact(PlayerController controller);

    void Detach(PlayerController controller);
}

public class TripodController : MonoBehaviour, Interactable, CastleInputActions.IPlayerActions
{
    public float rotateSpeed = 10;
    public Camera camera;
    public Transform headYTransform;
    public Transform headXTransform;

    public Casset[] cassets = new Casset[3];
    [SerializeField] CameraCassetVisual[] visualCassets = new CameraCassetVisual[3];

    [SerializeField] float upperCameraRotation = 55;
    [SerializeField] float lowerCameraRotation = -35;

    bool dirtyPosition = false;
    bool dirtyCaptures = false;
    CastleInputActions inputActions;
    CastleInputActions.PlayerActions playerActions;
    PlayerController controller;

    public void Interact(PlayerController playerController)
    {
        controller = playerController;
        controller.DeactiveCamera();
        controller.DeactiveControlls();
        camera.enabled = true;
        playerActions.Enable();
    }

    public void Detach(PlayerController playerController)
    {
        controller.ActiveCamera();
        controller.ActivateControlls();
        camera.enabled = false;
        playerActions.Disable();
        controller = null;
    }

    void Awake()
    {
        camera.enabled = false;
        inputActions = new();
        playerActions = inputActions.Player;
        playerActions.SetCallbacks(this);
        playerActions.Disable();
    }

    void Start()
    {
        for (int i = 0; i < visualCassets.Length; i++)
        {
            if (cassets[i] != null)
            {
                visualCassets[i].Transition(cassets[i].type);
            }
            else
            {
                visualCassets[i].Transition(Projectable.Type.none);
            }
        }
    }


    List<Capture.CapturedObject> objectWithViewportMatrix = new(); // ordered by Capture1.list - > Capture2.List
    void FixedUpdate()
    {
        if (dirtyCaptures)
        {
            InstantiateNewCaptures();
        }

        if (dirtyPosition)
        {
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        foreach (var capturedObject in objectWithViewportMatrix)
        {
            Matrix4x4 newMatrix = camera.transform.localToWorldMatrix * capturedObject.cameraSpace;
            capturedObject.copy.transform.position = newMatrix.GetPosition();

            if (newMatrix.ValidTRS())
            {
                capturedObject.copy.transform.rotation = newMatrix.rotation;
                //capturedObject.copy.transform.localScale = newMatrix.lossyScale;
            }

        }
        dirtyPosition = false;
    }

    private void InstantiateNewCaptures()
    {
        foreach (var capturedObject in objectWithViewportMatrix)
        {
            Destroy(capturedObject.copy);
        }

        objectWithViewportMatrix.Clear();

        foreach (var capture in cassets)
        {
            if (capture is ProjectionCasset projectionCasset && projectionCasset.captureToProject.isValid)
            {
                foreach (var capturedObject in projectionCasset.captureToProject.viewportSpaceCopy)
                {

                    var newCaptureObject = new Capture.CapturedObject
                    {
                        copy = Instantiate(capturedObject.copy),
                        cameraSpace = capturedObject.cameraSpace
                    };

                    newCaptureObject.projectable = newCaptureObject.copy.GetComponent<Projectable>();

                    objectWithViewportMatrix.Add(newCaptureObject);

                    newCaptureObject.projectable.IsProjected = true; // applied by something because in frustrum? 
                }
            }
        }

        dirtyPosition = true;
        dirtyCaptures = false;
    }

    bool AddCasset(int slot, Casset casset)
    {
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }

        dirtyCaptures = true;
        cassets[slot] = casset;
        visualCassets[slot].Transition(casset.type);
        controller.uIInventory.UpdateUI(controller);
        return true;
    }

    bool RemoveCasset(int slot, out Casset casset)
    {
        casset = null;
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }

        dirtyCaptures = true;
        casset = cassets[slot];
        cassets[slot] = null;
        visualCassets[slot].Transition(Projectable.Type.none);
        controller.uIInventory.UpdateUI(controller);
        return true; ;
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        headYTransform.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime * context.ReadValue<Vector2>().x, Space.World);

        var rotation = headXTransform.localEulerAngles;
        rotation.x += rotateSpeed * Time.deltaTime * context.ReadValue<Vector2>().y;
        rotation.x = clampAngle(rotation.x, lowerCameraRotation, upperCameraRotation);
        headXTransform.localEulerAngles = rotation;

        float clampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }

        dirtyPosition = true;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }

        Detach(PlayerController.instance); // yuck
    }

    public void OnJump(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }

    public void OnTakePhotograph(InputAction.CallbackContext context)
    {
    }

    public void OnActivateCamera(InputAction.CallbackContext context)
    {
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        SceneQuery.instance.ReloadScene();
    }

    public void OnCassetSelection1(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        switchCassets(0);
        controller.uIInventory.UpdateUI(controller);
    }

    public void OnCassetSelection2(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        switchCassets(1);
        controller.uIInventory.UpdateUI(controller);
    }

    public void OnCassetSelection3(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        switchCassets(2);
        controller.uIInventory.UpdateUI(controller);
    }

    void switchCassets(int playerInventorySelection)
    {

        if (cassets[playerInventorySelection] == null && controller.currentSelectedCasset is { } validCasset)
        {
            if (AddCasset(playerInventorySelection, validCasset))
            {
                controller.cassetInventory.Remove(validCasset);
            }
            return;
        }

        if (cassets[playerInventorySelection] != null)
        {
            if (RemoveCasset(playerInventorySelection, out var removedCasset))
            {
                if (controller.currentSelectedCasset is not { } valid)
                {
                    controller.cassetInventory.Add(removedCasset);
                }
                else
                {
                    removedCasset.DropCasset(controller.transform.position);
                }
            }
            return;
        }
    }
}
