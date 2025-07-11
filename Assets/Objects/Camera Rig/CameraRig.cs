using UnityEngine;

public class CameraRig : MonoBehaviour
{
    BasePlayer Target;
    public void SetTarget(BasePlayer value)
    {
        Target = value;

        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        var destination = Target.transform.position;

        destination.z = 0f;

        transform.position = destination;
    }
}