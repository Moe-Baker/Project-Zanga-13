using UnityEngine;

public class TimeSeed : MonoBehaviour
{
    [SerializeField]
    SphereCollider Collider;

    public GrabItem GrabItem { get; private set; }

    void Awake()
    {
        GrabItem = GetComponent<GrabItem>();
    }

    public bool CanGrow()
    {
        if (GrabItem.IsPickedUp)
            return false;

        return true;
    }

    public Vector3 GetTreeGrowthPosition()
    {
        var position = transform.position;

        position += Vector3.down * Collider.radius;

        return position;
    }
}
