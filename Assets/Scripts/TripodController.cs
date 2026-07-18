using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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

    [Header("Presets")]
    public Casset[] cassets = new Casset[3];
    public Lock[] poweredBy = new Lock[0];

    [Header("Config")]
    public float rotateSpeed = 10;
    [SerializeField] float upperCameraRotation = 55;
    [SerializeField] float lowerCameraRotation = -35;
    public Camera camera;
    public Transform headYTransform;
    public Transform headXTransform;
    [SerializeField] CameraCassetVisual[] visualCassets = new CameraCassetVisual[3];
    [SerializeField] MeshRenderer[] poweredMeshRenderers = new MeshRenderer[0];
    [SerializeField] MeshFilter frustrumMeshFilter;
    [SerializeField] Light frustrumSpotlight;
    [SerializeField] ParticleSystem frustrumParticleSystem;

    float startRotationSpeed = 10;
    bool powered = true;
    bool dirtyPosition = true;
    bool dirtyCaptures = true;
    CastleInputActions inputActions;
    CastleInputActions.PlayerActions playerActions;
    PlayerController controller;
    Mesh frustrumMesh;

    public void Interact(PlayerController playerController)
    {
        frustrumParticleSystem.Stop();
        controller = playerController;
        controller.DeactiveCamera();
        controller.DeactiveControlls();
        camera.enabled = true;
        playerActions.Enable();
    }

    public void Detach(PlayerController playerController)
    {
        updatePowerSource();
        controller.ActiveCamera();
        controller.ActivateControlls();
        camera.enabled = false;
        playerActions.Disable();
        controller = null;
    }

    void Awake()
    {
        startRotationSpeed = rotateSpeed;
        camera.enabled = false;
        inputActions = new();
        playerActions = inputActions.Player;
        frustrumSpotlight = GetComponentInChildren<Light>();
        frustrumSpotlight.spotAngle = camera.fieldOfView;
        playerActions.SetCallbacks(this);
        playerActions.Disable();
        frustrumMesh = new();
        frustrumMesh.MarkDynamic();
    }

    void Start()
    {
        foreach (var powerSource in poweredBy)
        {
            powerSource.onLockChanged += _ => { updatePowerSource(); };
        }

        for (int i = 0; i < cassets.Length; i++)
        {
            if (cassets[i].type == Projectable.Type.projection)
            {
                if (cassets[i].captureToProject.projectable != null)
                {
                    //Preset at startup should be removed
                    foreach (var item in cassets[i].captureToProject.projectable)
                    {
                        item.gameObject.SetActive(false);
                    }
                }

                cassets[i].captureToProject = Capture.CaptureProjecableWithCamera(cassets[i].captureToProject.projectable, camera);

            }
        }

        for (int i = 0; i < visualCassets.Length; i++)
        {
            visualCassets[i].initWith(cassets[i].type);
        }

        updatePowerSource();
    }

    void updatePowerSource()
    {
        powered = true;
        foreach (var item in poweredBy)
        {
            powered = !item.isLocked && powered;
        }

        if (powered)
        {
            frustrumParticleSystem.Play();
        }
        else
        {
            frustrumParticleSystem.Stop();
        }

        frustrumSpotlight.enabled = powered;

        rotateSpeed = powered ? startRotationSpeed : 0;

        foreach (var item in poweredMeshRenderers)
        {
            item.material = powered ? SceneQuery.instance.dB.poweredMaterial : SceneQuery.instance.dB.unPoweredMaterial;
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
        }
        Vector3[] innerCorners = new Vector3[4];
        camera.CalculateFrustumCorners(camera.rect, camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, innerCorners);

        Vector3[] outerCorners = new Vector3[4];
        camera.CalculateFrustumCorners(camera.rect, camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, outerCorners);

        int[] index =
        {
          0,1,2,
          2,3,0,
          0,5,1,
          0,4,1,
          1,5,6,
          6,2,1,
          2,6,7,
          7,3,2,
          0,4,3,
          3,4,7
        };

        List<Vector3> vertices = new();
        vertices.AddRange(innerCorners);
        vertices.AddRange(outerCorners);
        frustrumMesh.SetVertices(vertices, 0, vertices.Count);
        frustrumMesh.SetIndices(index, MeshTopology.Triangles, 0);
        frustrumMesh.MarkModified();

        frustrumMeshFilter.mesh = frustrumMesh;
        frustrumMeshFilter.GetComponent<MeshCollider>().sharedMesh = frustrumMesh;
        frustrumSpotlight.spotAngle = camera.fieldOfView;
        dirtyPosition = false;
    }

    void OnDrawGizmosSelected()
    {
        foreach (var item in cassets)
        {
            if (item.type == Projectable.Type.projection && item.captureToProject.isValid)
            {
                if (item.captureToProject.projectable == null) { continue; }
                foreach (var projecting in item.captureToProject.projectable)
                {
                    Gizmos.DrawLine(projecting.transform.position, transform.position);
                }
            }
        }
    }

    private void InstantiateNewCaptures()
    {

        foreach (var capturedObject in objectWithViewportMatrix)
        {
            Destroy(capturedObject.gameObject);
        }

        objectWithViewportMatrix.Clear();


        if (!powered)
        {
            frustrumParticleSystem.Stop();
            frustrumSpotlight.enabled = false;
            return;
        }

        frustrumSpotlight.enabled = true;
        frustrumSpotlight.color = Color.grey;

        foreach (var capture in cassets)
        {
            if (capture.type == Projectable.Type.projection && capture.captureToProject.isValid)
            {
                var capturedMesh = capture.captureToProject.capturedMesh;

                var projection = Instantiate(SceneQuery.instance.dB.projectable, camera.transform);
                var projectable = projection.GetComponent<Projectable>();
                projectable.filter.mesh = capturedMesh;
                projectable.originalVertices = capturedMesh.vertices;
                projectable.IsProjected = true;

                objectWithViewportMatrix.Add(projectable);
            }
        }

        Vector4 avgColor = default;
        foreach (var capture in cassets)
        {
            if (capture.type == Projectable.Type.solid)
            {
                foreach (var projection in objectWithViewportMatrix)
                {
                    projection.IsSolid = true;
                }
            }

            if (capture.type == Projectable.Type.rigid)
            {
                foreach (var projection in objectWithViewportMatrix)
                {
                    projection.IsRigid = true;
                }
            }


            if (capture.type != Projectable.Type.none)
            {
                frustrumParticleSystem.Play();

                avgColor += (Vector4)SceneQuery.instance.dB.getMaterial(capture.type).color;
                avgColor *= .3333f;
                avgColor.w = 1;
                frustrumSpotlight.enabled = true;
                frustrumSpotlight.color = avgColor;


                var main = frustrumParticleSystem.main;
                main.startColor = (Color)avgColor;
            }
        }

        dirtyPosition = true;
        dirtyCaptures = false;
    }

    public bool AddCasset(int slot, Casset casset)
    {
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }
        if (cassets[slot].type != Projectable.Type.none) { return false; }

        dirtyCaptures = true;
        cassets[slot] = casset;
        visualCassets[slot].Transition(casset.type);
        return true;
    }

    public bool RemoveCasset(int slot, out Casset casset)
    {
        casset = default;
        if (slot > cassets.Length) { return false; }
        if (slot < 0) { return false; }

        dirtyCaptures = true;
        casset = cassets[slot];
        cassets[slot] = default;
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
        if (cassets[slot].type == Projectable.Type.none && controller.currentlyHeldCasset.type != Projectable.Type.none)
        {
            if (AddCasset(slot, controller.currentlyHeldCasset))
            {
                controller.currentlyHeldCasset = default;
                controller.uiInventory.UpdateUI(controller);
            }
            return;
        }

        if (cassets[slot].type != Projectable.Type.none)
        {
            if (RemoveCasset(slot, out var removedCasset))
            {
                if (controller.currentlyHeldCasset.type == Projectable.Type.none)
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
