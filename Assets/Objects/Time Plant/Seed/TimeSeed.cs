using UnityEngine;

public class TimeSeed : MonoBehaviour
{
    [SerializeField]
    SphereCollider Collider;

    public Vector3 GetTreeGrowthPosition()
    {
        var position = transform.position;

        position += Vector3.down * Collider.radius;

        return position;
    }
}
