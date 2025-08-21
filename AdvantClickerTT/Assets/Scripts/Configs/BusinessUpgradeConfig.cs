using System;
using UnityEngine;

[Serializable]
public sealed class BusinessUpgradeConfig
{
    [SerializeField] private long _price;
    [SerializeField] private float _multiplier;
    // [SerializeField] private int _unlockLevel;

    public long Price => _price;
    public float Multiplier => _multiplier;
}