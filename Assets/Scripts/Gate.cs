using TMPro;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public Lock listenerLock;
    public GameObject lockedGate;
    public GameObject unlockedGate;

    public BoxCollider collider;
    void Start()
    {
        listenerLock.onLockChanged += (bool v) =>
        {
            ToggleGate(v);
        };

        lockedGate.GetComponent<MeshRenderer>().material = SceneQuery.instance.dB.lockedMaterial;
        unlockedGate.GetComponent<MeshRenderer>().material = SceneQuery.instance.dB.unLockedMaterial;
    }

    void ToggleGate(bool locked)
    {
        lockedGate.SetActive(locked);
        unlockedGate.SetActive(!locked);

        collider.enabled = locked;
    }

}
