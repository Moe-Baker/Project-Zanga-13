using UnityEngine;

public class TimePlant : MonoBehaviour
{
    [field: SerializeField]
    public TimeSeed Seed { get; private set; }

    [field: SerializeField]
    public TimeTree Tree { get; private set; }

    Level Level => Level.Instance;
    TimeSystem TimeSystem => Level.TimeSystem;

    void Start()
    {
        TimeSystem.OnChangePeriod += TimePeriodChangeCallback;
        TimePeriodChangeCallback(TimeSystem.Current.Period);
    }
    void OnDestroy()
    {
        TimeSystem.OnChangePeriod -= TimePeriodChangeCallback;
    }

    void TimePeriodChangeCallback(TimePeriod period)
    {
        Seed.gameObject.SetActive(period is TimePeriod.Past);

        if (period is TimePeriod.Future && Seed.CanGrow())
        {
            Tree.gameObject.SetActive(true);

            var position = Seed.GetTreeGrowthPosition();
            Tree.Grow(position);
        }
        else
        {
            Tree.gameObject.SetActive(false);
        }
    }
}