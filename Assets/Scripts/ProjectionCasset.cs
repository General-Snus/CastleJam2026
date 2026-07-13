using UnityEngine;

[CreateAssetMenu(fileName = "ProjectionCasset", menuName = "Scriptable Objects/ProjectionCasset")]
public class ProjectionCasset : Casset
{
    public Capture captureToProject;

    public override Projectable.Type type => Projectable.Type.projection;

    public override void OnEnterFrustrum(Projectable inFrustrum)
    {
        //We do nothing
    }

    public override void OnPlacedInProjector(TripodController projector)
    {

    }
}
