using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string PlayerBalance;
    public float GameTime;
    public List<BusinessData> BusinessSaveData = new();
}

[Serializable]
public class BusinessData
{
    public int Level;
    public int UpgradesMask;
    public float CycleStartTime;
    public float NextIncomeTime;
    public float FullCycleTime;
}