using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public interface Interactable
{
    void Interact(PlayerController controller);

    void Detach(PlayerController controller);

    void OnHover(PlayerController controller);
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
                visualCassets[i].initWith(cassets[i].type);
            }
            else
            {
                visualCassets[i].initWith(Projectable.Type.none);
            }
        }
    }


    List<Projectable> objectWithViewportMatrix = new(); // ordered by Capture1.list - > Capture2.List
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


    List<Vector3> modifiedMeshVertices = new();
    private void UpdatePositions()
    {
        foreach (var capturedObject in objectWithViewportMatrix)
        {
            modifiedMeshVertices.Clear();
            capturedObject.transform.localPosition = Vector3.zero;// Vector3.forward * camera.fieldOfView;
            capturedObject.transform.localRotation = Quaternion.identity;
            capturedObject.transform.localScale = Vector3.one;//* 10 * camera.fieldOfView;


            foreach (var item in capturedObject.originalVertices)
            {
                modifiedMeshVertices.Add(camera.projectionMatrix.inverse.MultiplyPoint(item));
            }

            capturedObject.mesh.vertices = modifiedMeshVertices.ToArray();
            capturedObject.mesh.RecalculateBounds();
            // Matrix4x4 newMatrix = camera.transform.localToWorldMatrix * capturedObject.cameraSpace;
            // capturedObject.copy.transform.position = newMatrix.GetPosition();
            //
            // if (newMatrix.ValidTRS())
            // {
            //     capturedObject.copy.transform.rotation = newMatrix.rotation;
            //     //capturedObject.copy.transform.localScale = newMatrix.lossyScale;
            // }

        }
        dirtyPosition = false;
    }

    private void InstantiateNewCaptures()
    {
        foreach (var capturedObject in objectWithViewportMatrix)
        {
            Destroy(capturedObject.gameObject);
        }

        objectWithViewportMatrix.Clear();



        foreach (var capture in cassets)
        {
            if (capture is ProjectionCasset projectionCasset && projectionCasset.captureToProject.isValid)
            {
                var captureObject = projectionCasset.captureToProject.capturedObject;

                var projection = Instantiate(SceneQuery.instance.dB.projectable, camera.transform);
                var projectable = projection.GetComponent<Projectable>();
                projectable.filter.mesh = captureObject.mesh;
                projectable.originalVertices = captureObject.mesh.vertices;
                projectable.IsProjected = true;

                objectWithViewportMatrix.Add(projectable);
            }
        }


        foreach (var capture in cassets)
        {
            if (capture is SolidCasset solidCasset)
            {
                foreach (var projection in objectWithViewportMatrix)
                {
                    projection.IsSolid = true;
                }
            }

            if (capture is RigidCasset rigidCasset)
            {
                foreach (var projection in objectWithViewportMatrix)
                {
                    projection.IsRigid = true;
                }
            }
        }

        dirtyPosition = true;
        dirtyCaptures = false;
    }

    public bool AddCasset(int slot, Casset casset)
    {
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }
        if (cassets[slot] != null) { return false; }

        dirtyCaptures = true;
        cassets[slot] = casset;
        visualCassets[slot].Transition(casset.type);
        return true;
    }

    public bool RemoveCasset(int slot, out Casset casset)
    {
        casset = null;
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }

        dirtyCaptures = true;
        casset = cassets[slot];
        cassets[slot] = null;
        visualCassets[slot].Transition(Projectable.Type.none);
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
        Cursor.lockState = CursorLockMode.Locked;
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
        switchCassets(controller, 0);
        controller.uiInventory.UpdateUI(controller);
    }

    public void OnCassetSelection2(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        switchCassets(controller, 1);
        controller.uiInventory.UpdateUI(controller);
    }

    public void OnCassetSelection3(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        switchCassets(controller, 2);
        controller.uiInventory.UpdateUI(controller);
    }

    public void switchCassets(PlayerController controller, int slot)
    {
        if (cassets[slot] == null && controller.currentlyHeldCasset != null)
        {
            if (AddCasset(slot, controller.currentlyHeldCasset))
            {
                controller.currentlyHeldCasset = null;
                controller.uiInventory.UpdateUI(controller);
            }
            return;
        }

        if (cassets[slot] != null)
        {
            if (RemoveCasset(slot, out var removedCasset))
            {
                if (controller.currentlyHeldCasset == null)
                {
                    controller.currentlyHeldCasset = removedCasset;
                }
                else
                {
                    removedCasset.DropCasset(controller.transform.position);
                }
            }
            controller.uiInventory.UpdateUI(controller);
            return;
        }
    }


    [SerializeField] float lowerFoW, upperFoW;
    public void OnZoom(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        camera.fieldOfView += context.ReadValue<Vector2>().y * Time.deltaTime * 25;


        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, lowerFoW, upperFoW);
        dirtyPosition = true;
    }

    public void OnHover(PlayerController controller)
    {

    }
}
