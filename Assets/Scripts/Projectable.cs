using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
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
    public bool IsKinematic
    {
        get
        {
            return _isKinematic;
        }
        set
        {
            _isKinematic = value;
            isDirty = true;
        }
    }

    bool isDirty = true;

    [SerializeField] bool _isProjected = false;
    [SerializeField] bool _isSolid = false;
    [SerializeField] bool _isKinematic = false;





    Rigidbody rb;
    BoxCollider collider;
    MeshRenderer renderer;
    public void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

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
        rb.isKinematic = true;

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

        if (IsKinematic)
        {
            rb.isKinematic = false;
            renderer.material = db.rigidProjectionMaterial;
        }

        if (!IsProjected)
        {
            renderer.material = db.projectableMaterial;
        }
    }

}
