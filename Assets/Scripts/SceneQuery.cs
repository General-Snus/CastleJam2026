using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneQuery : MonoBehaviour
{
    public static SceneQuery instance;

    public DB dB;

    public Interactable[] tripodControllers = new Interactable[0];
    public Projectable[] projectable = new Projectable[0];

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }

        instance = this;

        projectable = FindObjectsByType<Projectable>(FindObjectsInactive.Exclude);
        tripodControllers = FindObjectsByType<TripodController>(FindObjectsInactive.Exclude);


    }


    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
