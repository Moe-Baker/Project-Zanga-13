using UnityEngine;

public static class GeneralExtensions
{
    public static GameObject GetRoot(this Collider collider)
    {
        if (collider.attachedRigidbody)
            return collider.attachedRigidbody.gameObject;
        else
            return collider.gameObject;
    }
}