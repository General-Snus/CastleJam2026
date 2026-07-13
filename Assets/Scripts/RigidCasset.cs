using UnityEngine;

[CreateAssetMenu(fileName = "RigidCasset", menuName = "Scriptable Objects/RigidCasset")]
public class RigidCasset : Casset
{
    public override Projectable.Type type => Projectable.Type.rigid;

    public override void OnEnterFrustrum(Projectable inFrustrum)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlacedInProjector(TripodController projector)
    {
        throw new System.NotImplementedException();
    }
}
