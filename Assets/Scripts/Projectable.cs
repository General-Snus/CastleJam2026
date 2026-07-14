using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
public class Projectable : MonoBehaviour
{
    public enum Type
    {
        none,
        projection,
        solid,
        rigid,
    }

    public bool IsProjected
    {
        get
        {
            return _isProjected;
        }
        set
        {
            _isProjected = value;
            isDirty = true;
        }
    }
    public bool IsSolid
    {
        get
        {
            return _isSolid;
        }
        set
        {
            _isSolid = value;
            isDirty = true;
        }
    }
    public bool IsRigid
    {
        get
        {
            return _isRigid;
        }
        set
        {
            _isRigid = value;
            isDirty = true;
        }
    }

    bool isDirty = true;

    [SerializeField] bool _isProjected = false;
    [SerializeField] bool _isSolid = false;
    [SerializeField] bool _isRigid = false;





    Rigidbody rb;
    MeshCollider collider;
    MeshRenderer renderer;
    public MeshFilter filter;
    public Mesh mesh => filter.mesh;
    public Vector3[] originalVertices;
    public void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<MeshCollider>();
        rb = GetComponent<Rigidbody>();
        filter = GetComponent<MeshFilter>();

        collider.enabled = false;
        rb.isKinematic = true;
        collider.sharedMesh = mesh;

    }

    void Start()
    {
        UpdateState();
    }

    void LateUpdate()
    {
        if (isDirty)
        {
            UpdateState();
        }
    }

    void UpdateState()
    {
        collider.enabled = false;
        collider.convex = false;
        rb.isKinematic = true;
        collider.sharedMesh = mesh;

        var db = SceneQuery.instance.dB;

        if (IsProjected)
        {
            renderer.material = db.projectionMaterial;
        }

        if (IsSolid)
        {
            collider.enabled = true;
            renderer.material = db.solidProjectionMaterial;
        }

        if (IsRigid)
        {
            rb.isKinematic = false;
            collider.convex = true;
            renderer.material = db.rigidProjectionMaterial;
        }

        if (!IsProjected)
        {
            renderer.material = db.projectableMaterial;
        }
    }

}
