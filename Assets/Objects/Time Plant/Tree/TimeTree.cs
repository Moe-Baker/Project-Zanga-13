using UnityEngine;

public class TimeTree : MonoBehaviour
{
    public void Grow(Vector3 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(new Vector3()
        {
            x = Random.Range(-1, 1),
            y = Random.Range(0f, 360f),
            z = Random.Range(-1, 1),
        });
    }
}