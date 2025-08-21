public static class BusinessUpgradesMaskExtensions
{
    public static bool HasUpgrade(this ref BusinessUpgradesComponent component, int index)
    {
        return (component.UpgradesMask & (1 << index)) != 0;
    }

    public static void AddUpgrade(this ref BusinessUpgradesComponent component, int index)
    {
        component.UpgradesMask = (byte)(component.UpgradesMask | (1 << index));
    }

    public static void RemoveUpgrade(this ref BusinessUpgradesComponent component, int index)
    {
        component.UpgradesMask = (byte)(component.UpgradesMask & ~(1 << index));
    }
}