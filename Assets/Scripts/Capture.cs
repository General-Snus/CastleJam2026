using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Capture
{

    [System.Serializable]
    public struct CapturedObject
    {
        public GameObject copy;
        public Projectable projectable;
        public Matrix4x4 cameraSpace;
    }

    [SerializeField] public List<CapturedObject> viewportSpaceCopy;
    public bool isValid => viewportSpaceCopy != null && viewportSpaceCopy.Count > 0;

    public static Capture CaptureWithCamera(Camera camera)
    {
        Capture capture = new()
        {
            viewportSpaceCopy = new()
        };

        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        foreach (var projectable in SceneQuery.instance.projectable)
        {
            var insideFrustrum = GeometryUtility.TestPlanesAABB(planes, projectable.GetComponentInChildren<Collider>().bounds);
            if (!insideFrustrum) { continue; }
            var viewspace = camera.transform.worldToLocalMatrix * projectable.transform.localToWorldMatrix;
            capture.viewportSpaceCopy.Add(new CapturedObject { copy = projectable.gameObject, projectable = projectable, cameraSpace = viewspace });
        }

        return capture;
    }
}
