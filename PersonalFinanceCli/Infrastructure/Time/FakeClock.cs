namespace PersonalFinanceCli.Infrastructure.Time;

public sealed class FakeClock : IClock
{
    public FakeClock(DateOnly today)
    {
        Today = today;
    }

    public DateOnly Today { get; set; }
}