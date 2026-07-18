using TMPro;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public bool _isLocked = true;
    public bool isLocked
    {
        get
        {
            return _isLocked;
        }
        set
        {
            if (value == _isLocked) { return; }
            _isLocked = value;
            onLockChanged.Invoke(value);
            updateVisual();
        }
    }

    public System.Action<bool> onLockChanged;

    public MeshRenderer[] lockRenderers = new MeshRenderer[0];
    public MeshRenderer[] powerRenderer = new MeshRenderer[0];
    void Start()
    {
        updateVisual();
    }

    void updateVisual()
    {
        foreach (var item in lockRenderers)
        {
            item.material = isLocked ? SceneQuery.instance.dB.lockedMaterial : SceneQuery.instance.dB.unLockedMaterial;
        }
        foreach (var item in powerRenderer)
        {
            item.material = isLocked ? SceneQuery.instance.dB.unPoweredMaterial : SceneQuery.instance.dB.poweredMaterial;
        }

    }

    void FixedUpdate()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        isLocked = !other.gameObject.TryGetComponent<Key>(out _);
    }
    public void OnTriggerExit(Collider other)
    {
        isLocked = !other.gameObject.TryGetComponent<Key>(out _);
    }

}
