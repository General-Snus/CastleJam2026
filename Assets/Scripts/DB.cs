using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DB", menuName = "Scriptable Objects/DB")]
public class DB : ScriptableObject
{
    public Material projectableMaterial;
    public Material projectionMaterial;
    public Material solidProjectionMaterial;
    public Material rigidProjectionMaterial;

    public Material lockedMaterial;
    public Material unLockedMaterial;
    
    public Material UI_projectableMaterial;
    public Material UI_projectionMaterial;
    public Material UI_solidProjectionMaterial;
    public Material UI_rigidProjectionMaterial;

    public Sprite emptyImage;
    public Sprite projectionImage;
    public Sprite solidImage;
    public Sprite rigidImage;

    public GameObject cassetPrefab;
    public GameObject projectable;

    public Sprite getSprite(Projectable.Type type)
    {
        return type switch
        {
            Projectable.Type.none => emptyImage,
            Projectable.Type.projection => emptyImage,
            Projectable.Type.solid => emptyImage,
            Projectable.Type.rigid => emptyImage,
            _ => emptyImage,
        };
    }

    public Material getMaterial(Projectable.Type type)
    {
        return type switch
        {
            Projectable.Type.none => projectableMaterial,
            Projectable.Type.projection => projectionMaterial,
            Projectable.Type.solid => solidProjectionMaterial,
            Projectable.Type.rigid => rigidProjectionMaterial,
            _ => projectableMaterial,
        };
    }

     public Material getUIMaterial(Projectable.Type type)
    {
        return type switch
        {
            Projectable.Type.none => UI_projectableMaterial,
            Projectable.Type.projection => UI_projectionMaterial,
            Projectable.Type.solid => UI_solidProjectionMaterial,
            Projectable.Type.rigid => UI_rigidProjectionMaterial,
            _ => UI_projectableMaterial,
        };
    }

}
