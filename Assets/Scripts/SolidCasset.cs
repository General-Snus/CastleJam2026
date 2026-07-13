using UnityEngine;

[CreateAssetMenu(fileName = "SolidCasset", menuName = "Scriptable Objects/SolidCasset")]
public class SolidCasset : Casset
{
    public override Projectable.Type type => Projectable.Type.solid;

    public override void OnEnterFrustrum(Projectable inFrustrum)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlacedInProjector(TripodController projector)
    {
        throw new System.NotImplementedException();
    }
}
