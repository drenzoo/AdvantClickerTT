using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NameConfig", menuName = "Configs/Game/Name Config")]
public sealed class NameConfig : ScriptableObject
{
    [SerializeField] private BusinessName[] _businesses;
    [SerializeField] private UpgradeName[] _upgrades;

    public string GetBusinessName(int businessId)
    {
        foreach (var b in _businesses)
        {
            if (b.BusinessId == businessId)
            {
                return b.Title;
            }
        }

        return $"Business {businessId}";
    }

    public string GetUpgradeName(int businessId, int index)
    {
        foreach (var u in _upgrades)
        {
            if (u.BusinessId == businessId && u.UpgradeIndex == index)
            {
                return u.Title;
            }
        }

        return $"Upgrade {businessId}.{index}";
    }
}

[Serializable]
public sealed class BusinessName
{
    [SerializeField] private int _businessId;
    [SerializeField] private string _title;

    public int BusinessId => _businessId;
    public string Title => _title;
}

[Serializable]
public sealed class UpgradeName
{
    [SerializeField] private int _businessId;
    [SerializeField] private int _upgradeIndex;
    [SerializeField] private string _title;

    public int BusinessId => _businessId;
    public int UpgradeIndex => _upgradeIndex;
    public string Title => _title;
}