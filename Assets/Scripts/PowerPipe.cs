using UnityEngine;

public class PowerPipe : MonoBehaviour
{
    public Lock[] listenerLock = new Lock[0];
    public MeshRenderer lineRenderer;

    void Start()
    {
        foreach (var locks in listenerLock)
        {
            locks.onLockChanged += _ => testForPower();
        }

        lineRenderer.material = SceneQuery.instance.dB.lockedMaterial;
        testForPower();
    }

    void testForPower()
    {
        foreach (var locks in listenerLock)
        {
            lineRenderer.material = locks.isLocked ? SceneQuery.instance.dB.unPoweredMaterial : SceneQuery.instance.dB.poweredMaterial;
        }
    }

}
