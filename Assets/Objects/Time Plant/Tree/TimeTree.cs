using UnityEngine;

public class TimeTree : MonoBehaviour
{
    public void Grow(Vector3 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
    }
}