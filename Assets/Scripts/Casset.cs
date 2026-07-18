using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct Casset
{
    public Capture captureToProject;
    public Projectable.Type type ;
}

public static class CassetExtensions
{
    public static void DropCasset(this Casset casset, Vector3 position)
    {
        var inWorldCasset = GameObject.Instantiate(SceneQuery.instance.dB.cassetPrefab, position, Quaternion.identity);
        inWorldCasset.GetComponentInChildren<CassetContainer>().casset = casset;
    }
}


