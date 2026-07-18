using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct Capture
{

    [System.Serializable]
    public struct CapturedObject
    {
        public Mesh mesh;
    }

    public Projectable[] projectable;
    [SerializeField] public Mesh capturedMesh;
    public bool isValid => capturedMesh != null;

    public static Capture CaptureProjecableWithCamera(Projectable[] buildFromProjectable, Camera camera)
    {
        Capture capture = new()
        {
            capturedMesh = new()
        };


        var newMesh = new Mesh();
        List<Vector3> newPoints = new();
        List<int> newIndicies = new();
        newMesh.subMeshCount = buildFromProjectable.Length;
        for (int i = 0; i < buildFromProjectable.Length; i++)
        {
            var projectable = buildFromProjectable[i];
            var viewspace = camera.projectionMatrix * camera.transform.worldToLocalMatrix * projectable.transform.localToWorldMatrix;

            var indicies = projectable.filter.mesh.GetIndices(0);
            var meshVertices = projectable.filter.mesh.vertices;

            for (int j = 0; j < meshVertices.Length; j++)
            {
                var vertex = meshVertices[j];
                vertex = viewspace.MultiplyPoint(vertex);
                newPoints.Add(vertex);
            }

            for (int j = 0; j < indicies.Length; j++)
            {
                var index = indicies[j];
                newIndicies.Add(index);
            }

            newMesh.vertices = newPoints.ToArray();
            newMesh.SetIndices(newIndicies, MeshTopology.Triangles, i, true);
        }
        capture.capturedMesh = newMesh;
        
        return capture;
    }

    public static Capture CaptureWithCamera(Camera camera)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        Capture capture = new()
        {
            capturedMesh = new()
        };


        var newMesh = new Mesh();
        List<Vector3> newPoints = new();
        List<int> newIndicies = new();
        newMesh.subMeshCount = SceneQuery.instance.projectable.Length;
        for (int i = 0; i < SceneQuery.instance.projectable.Length; i++)
        {
            Projectable projectable = SceneQuery.instance.projectable[i];
            var insideFrustrum = GeometryUtility.TestPlanesAABB(planes, projectable.GetComponentInChildren<Collider>().bounds);
            if (!insideFrustrum) { continue; }
            var viewspace = camera.projectionMatrix * camera.transform.worldToLocalMatrix * projectable.transform.localToWorldMatrix;

            var indicies = projectable.filter.mesh.GetIndices(0);
            var meshVertices = projectable.filter.mesh.vertices;

            for (int j = 0; j < meshVertices.Length; j++)
            {
                var vertex = meshVertices[j];
                vertex = viewspace.MultiplyPoint(vertex);



                newPoints.Add(vertex);
            }

            for (int j = 0; j < indicies.Length; j++)
            {
                var index = indicies[j];
                newIndicies.Add(index);
            }
            newMesh.vertices = newPoints.ToArray();
            newMesh.SetIndices(newIndicies, MeshTopology.Triangles, i, true);
        }
        capture.capturedMesh = newMesh;

        return capture;
    }
}
