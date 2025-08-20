using UnityEngine;

[System.Serializable]
public sealed class BusinessConfig
{
    [SerializeField] private int _id;
    [SerializeField] private float _delaySeconds;
    [SerializeField] private long _baseCost;
    [SerializeField] private long _baseIncome;
    [SerializeField] private BusinessUpgradeConfig _upgrade1;
    [SerializeField] private BusinessUpgradeConfig _upgrade2;

    public int Id => _id;
    public long BaseCost => _baseCost;
    public long BaseIncome => _baseIncome;
    public float DelaySeconds => _delaySeconds;
    public BusinessUpgradeConfig Upgrade1 => _upgrade1;
    public BusinessUpgradeConfig Upgrade2 => _upgrade2;
}