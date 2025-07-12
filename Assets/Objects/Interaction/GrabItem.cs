using NUnit.Framework;

using System;
using System.Collections.Generic;

using UnityEngine;

public class GrabItem : InteractionItem
{
    [field: SerializeField]
    public Rigidbody Rigidbody { get; private set; }

    public Collider[] Colliders { get; private set; }

    bool IsKinematic;

    Player Owner;
    public bool IsPickedUp => Owner != null;

    void Awake()
    {
        IsKinematic = Rigidbody.isKinematic;

        Colliders = GetComponentsInChildren<Collider>(true);
    }

    public void Pickup(Player Owner)
    {
        this.Owner = Owner;

        Rigidbody.isKinematic = true;
        SetCollidersState(false);

        OnPickup?.Invoke(Owner);
    }
    public event Action<Player> OnPickup;

    public void Drop()
    {
        var cache = Owner;
        Owner = default;

        Rigidbody.isKinematic = IsKinematic;
        SetCollidersState(true);

        OnDrop?.Invoke(cache);
    }
    public event Action<Player> OnDrop;

    void SetCollidersState(bool enabled)
    {
        foreach (var collider in Colliders)
            collider.enabled = enabled;
    }
}