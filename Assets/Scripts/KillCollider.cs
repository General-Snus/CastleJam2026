using UnityEngine;

public class KillCollider : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var _))
        {
            SceneQuery.instance.ReloadScene();
        }
    }
}
