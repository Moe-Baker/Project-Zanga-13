using System;

using UnityEngine;

public abstract class TimeStream : MonoBehaviour
{
    public TimePeriod Period
    {
        get
        {
            if (this == TimeSystem.Streams.Future)
                return TimePeriod.Future;

            if (this == TimeSystem.Streams.Past)
                return TimePeriod.Past;

            throw new NotImplementedException();
        }
    }

    Level Level => Level.Instance;
    TimeSystem TimeSystem => Level.TimeSystem;

    public bool InUse => TimeSystem.Current == this;

    public virtual void Enter()
    {
        OnEnter?.Invoke();
    }
    public event Action OnEnter;

    public virtual void Exit()
    {
        OnExit?.Invoke();
    }
    public event Action OnExit;
}