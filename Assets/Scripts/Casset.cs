using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;

public class Casset : ScriptableObject
{
    public virtual Projectable.Type type
    {
        get;
    }
    public virtual void OnEnterFrustrum(Projectable inFrustrum) { }
    public virtual void OnPlacedInProjector(TripodController projector) { }
}

public static class CassetExtensions
{
    public static void DropCasset(this Casset casset, Vector3 position)
    {
        var inWorldCasset = GameObject.Instantiate(SceneQuery.instance.dB.cassetPrefab, position, Quaternion.identity);
        inWorldCasset.GetComponentInChildren<CassetContainer>().casset = casset;
    }
}


