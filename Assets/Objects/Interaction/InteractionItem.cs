using System;

using UnityEngine;

public class InteractionItem : MonoBehaviour
{
    [field: SerializeField]
    public float Radius { get; private set; } = 1f;

    [field: SerializeField]
    public string Instructions { get; private set; } = "Interact";

    public void Interact(Player Player)
    {
        Debug.Log($"Player {Player} Interacted With Item");

        OnInteract?.Invoke(Player);
    }
    public event Action<Player> OnInteract;

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}