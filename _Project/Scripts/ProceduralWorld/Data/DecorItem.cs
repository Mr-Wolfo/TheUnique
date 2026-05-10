using UnityEngine;

[System.Serializable]
public struct DecorItem
{
    public GameObject Prefab;
    public Vector3 LocalOffset;
    public Quaternion Rotation;

    public string GetStableId()
    {
        return Prefab != null ? Prefab.name : "NULL";
    }
}