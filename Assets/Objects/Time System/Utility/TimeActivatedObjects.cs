using UnityEngine;

public class TimeActivatedObjects : MonoBehaviour
{
    public TimePeriodData<GameObject[]> Objects;

    Level Level => Level.Instance;
    TimeSystem TimeSystem => Level.TimeSystem;

    void Start()
    {
        TimeSystem.OnChangePeriod += PeriodChangeCallback;
        UpdateState();
    }

    void PeriodChangeCallback(TimePeriod period) => UpdateState();
    void UpdateState()
    {
        foreach (var entry in Objects.Past)
            entry.SetActive(TimeSystem.Current.Period is TimePeriod.Past);

        foreach (var entry in Objects.Future)
            entry.SetActive(TimeSystem.Current.Period is TimePeriod.Future);
    }
}