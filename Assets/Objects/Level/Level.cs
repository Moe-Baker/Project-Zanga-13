using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}