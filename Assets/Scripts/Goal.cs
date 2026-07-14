using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public List<Lock> locks = new();
    public MeshRenderer renderer;
    public bool unlocked
    {
        get
        {
            foreach (var item in locks)
            {
                if (item.isLocked) { return false; }
            }

            return true;
        }
    }


    void Start()
    {
        renderer.material = !unlocked ? SceneQuery.instance.dB.lockedMaterial : SceneQuery.instance.dB.unLockedMaterial;
    }
}
