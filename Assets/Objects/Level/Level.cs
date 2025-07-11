using UnityEngine;

public class Level : MonoBehaviour
{
    [field: SerializeField]
    public TimeSystem TimeSystem { get; private set; }

    public static Level Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}