using System;

using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    [field: SerializeField]
    public TimePeriodData<TimeStream> Streams { get; private set; }

    public TimeStream Current { get; private set; }

    public void ChangePeriod(TimePeriod value)
    {
        Current?.Exit();
        Current = Streams.Evaluate(value);
        Current?.Enter();

        OnChangePeriod?.Invoke(value);
    }
    public event Action<TimePeriod> OnChangePeriod;

    void Awake()
    {
        ChangePeriod(TimePeriod.Past);
    }
    void Start()
    {

    }

    public void TogglePeriod()
    {
        var value = Current.Period;
        value = TogglePeriod(value);
        ChangePeriod(value);
    }

    public static TimePeriod TogglePeriod(TimePeriod value) => value switch
    {
        TimePeriod.Past => TimePeriod.Future,
        TimePeriod.Future => TimePeriod.Past,

        _ => throw new NotImplementedException(),
    };
}

public enum TimePeriod
{
    Past, Future
}

[Serializable]
public struct TimePeriodData<TData>
{
    [field: SerializeField]
    public TData Past { get; private set; }

    [field: SerializeField]
    public TData Future { get; private set; }

    public TData Evaluate(TimePeriod period) => period switch
    {
        TimePeriod.Past => Past,
        TimePeriod.Future => Future,

        _ => throw new NotImplementedException(),
    };

    public void ForAll(Action<TData> action)
    {
        action(Past);
        action(Future);
    }

    public void ForAll(Action<TimePeriod, TData> action)
    {
        action(TimePeriod.Past, Past);
        action(TimePeriod.Future, Future);
    }

    public TimePeriodData(TData Past, TData Future)
    {
        this.Past = Past;
        this.Future = Future;
    }
}