using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Start()
    {
        if(playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is somehow not assigned in spawner");
            return;
        }

        var player = Instantiate(playerPrefab);        
        player.transform.position = transform.position;
        player.transform.rotation = Quaternion.identity;
    }
}
