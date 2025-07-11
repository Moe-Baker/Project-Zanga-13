using UnityEngine;

public class CameraRig : MonoBehaviour
{
    Player Target;
    public void SetTarget(Player value)
    {
        Target = value;

        transform.parent = Target.transform.parent;
        transform.rotation = Quaternion.identity;
    }

    public void SetActive(bool value) => gameObject.SetActive(value);

    void LateUpdate()
    {
        var destination = Target.transform.position;

        //destination.z = 0f;

        transform.position = destination;
    }
}