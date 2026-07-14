using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CameraCassetVisual : MonoBehaviour
{
    [SerializeField] MeshRenderer filmObject;
    [SerializeField] Transform targetIn;
    [SerializeField] Transform targetOut;
    [SerializeField] float transitionTime = .5f;
    Projectable.Type type;
    public void Transition(Projectable.Type type)
    {
        StartCoroutine(transition(type));
    }

    public void initWith(Projectable.Type newType)
    {
        type = newType;
        Vector3 target = targetIn.position;
        if (newType == Projectable.Type.none)
        {
            target = targetOut.position;
        }

        filmObject.transform.position = target;

        filmObject.enabled = true;
        switch (newType)
        {
            case Projectable.Type.none:
                filmObject.enabled = false;
                break;
            case Projectable.Type.projection:
                filmObject.material = SceneQuery.instance.dB.projectionMaterial;
                break;
            case Projectable.Type.solid:
                filmObject.material = SceneQuery.instance.dB.solidProjectionMaterial;
                break;
            case Projectable.Type.rigid:
                filmObject.material = SceneQuery.instance.dB.rigidProjectionMaterial;
                break;
        }
    }
    IEnumerator transition(Projectable.Type newType)
    {
        if (type == newType) { yield break; }
        type = newType;
        Vector3 target = targetIn.position;
        bool outAndIn = type != Projectable.Type.none;
        if (newType == Projectable.Type.none)
        {
            target = targetOut.position;
        }

        filmObject.transform.position = target;

        filmObject.enabled = true;
        switch (newType)
        {
            case Projectable.Type.none:
                filmObject.enabled = false;
                break;
            case Projectable.Type.projection:
                filmObject.material = SceneQuery.instance.dB.projectionMaterial;
                break;
            case Projectable.Type.solid:
                filmObject.material = SceneQuery.instance.dB.solidProjectionMaterial;
                break;
            case Projectable.Type.rigid:
                filmObject.material = SceneQuery.instance.dB.rigidProjectionMaterial;
                break;
        }
        //var velocity = Vector3.zero;
        //var t = Time.time + transitionTime;
        //while (Time.time < t)
        //{
        //}








    }



}
