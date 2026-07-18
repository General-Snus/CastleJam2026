using UnityEngine;

public class CassetContainer : MonoBehaviour, Interactable
{
    public Casset casset;
    public MeshRenderer renderer;

    public void Detach(PlayerController controller)
    {
    }

    public void Interact(PlayerController controller)
    {
        if (controller.currentlyHeldCasset.type != Projectable.Type.none) { return; }
        controller.currentlyHeldCasset = casset;
        Destroy(this.gameObject);
    }

    void Start()
    {
        adjustMaterial();
    }

    private void adjustMaterial()
    {
        switch (casset.type)
        {
            case Projectable.Type.none:
                break;
            case Projectable.Type.projection:
                renderer.material = SceneQuery.instance.dB.projectionMaterial;
                break;
            case Projectable.Type.solid:
                renderer.material = SceneQuery.instance.dB.solidProjectionMaterial;
                break;
            case Projectable.Type.rigid:
                renderer.material = SceneQuery.instance.dB.rigidProjectionMaterial;
                break;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 8);
    }

    public void OnHover(PlayerController controller)
    {
    }
}
