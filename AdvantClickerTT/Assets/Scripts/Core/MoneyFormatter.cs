public class MoneyFormatter
{
    public string Format(decimal value)
    {
        return value switch
        {
            >= 1_000_000_000 => (value / 1_000_000_000m).ToString("0.##") + "B",
            >= 1_000_000     => (value / 1_000_000m).ToString("0.##") + "M",
            >= 1_000         => (value / 1_000m).ToString("0.##") + "K",
            _                => value.ToString("0.##")
        };
    }
}