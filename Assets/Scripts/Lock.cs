using UnityEngine;

public class Lock : MonoBehaviour
{
    public bool isLocked = true;

    public MeshRenderer renderer;
    void Start()
    {
        renderer.material = isLocked ? SceneQuery.instance.dB.lockedMaterial : SceneQuery.instance.dB.unLockedMaterial;
    }
}
