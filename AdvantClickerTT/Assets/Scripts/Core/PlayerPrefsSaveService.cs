using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using UnityEngine;

public sealed class PlayerPrefsSaveService : ISaveService
{
    private const string PrefsSaveKey = "game_save";

    public void Save(EcsWorld world)
    {
        var saveData = new SaveData();

        var playerCurrencyPool = world.GetPool<PlayerCurrencyComponent>();
        var currencyFilter = world.Filter<PlayerCurrencyComponent>().End();

        foreach (var entity in currencyFilter)
        {
            var balance = playerCurrencyPool.Get(entity).CurrentBalance;
            saveData.PlayerBalance = balance.ToString(System.Globalization.CultureInfo.InvariantCulture);
            break;
        }

        var gameTimePool = world.GetPool<GameTimeComponent>();
        var gameTimeFilter = world.Filter<GameTimeComponent>().End();

        foreach (var entity in gameTimeFilter)
        {
            saveData.GameTime = gameTimePool.Get(entity).CurrentTime;
            break;
        }

        var businessLevelPool = world.GetPool<BusinessLevelComponent>();
        var businessUpgradesPool = world.GetPool<BusinessUpgradesComponent>();
        var incomeCyclePool = world.GetPool<IncomeCycleComponent>();
        var businessLevelFilter = world.Filter<BusinessLevelComponent>().End();

        foreach (var entity in businessLevelFilter)
        {
            var businessData = new BusinessData
            {
                Level = (int)businessLevelPool.Get(entity).Level,
                UpgradesMask = businessUpgradesPool.Has(entity) ? businessUpgradesPool.Get(entity).UpgradesMask : 0,
                CycleStartTime = incomeCyclePool.Has(entity) ? incomeCyclePool.Get(entity).CycleStartTime : 0f,
                NextIncomeTime = incomeCyclePool.Has(entity) ? incomeCyclePool.Get(entity).NextIncomeTime : 0f,
                FullCycleTime = incomeCyclePool.Has(entity) ? incomeCyclePool.Get(entity).FullCycleTime : 0f
            };
            saveData.BusinessSaveData.Add(businessData);
        }

        var json = JsonUtility.ToJson(saveData);

        PlayerPrefs.SetString(PrefsSaveKey, json);
        PlayerPrefs.Save();

#if UNITY_EDITOR
        UnityEngine.Debug.Log($"[PlayerPrefsSaveService]: SAVE: [{json}]");
#endif
    }

    public bool TryLoad(EcsWorld world)
    {
        if (!PlayerPrefs.HasKey(PrefsSaveKey))
        {
            return false;
        }

        var json = PlayerPrefs.GetString(PrefsSaveKey);

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        SaveData saveData;

        try
        {
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogWarning($"[PlayerPrefsSaveService]: Broken save, ignoring. ex: {exception}");
            return false;
        }
#if UNITY_EDITOR
        finally
        {
            UnityEngine.Debug.Log("[PlayerPrefsSaveService]: Done");
        }
#endif

        if (saveData == null)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[PlayerPrefsSaveService]: No save found");
#endif
            return false;
        }

        if (saveData.BusinessSaveData == null)
        {
            UnityEngine.Debug.LogError("[PlayerPrefsSaveService]: Save data is corrupted.");
            return false;
        }

        var playerCurrencyPool = world.GetPool<PlayerCurrencyComponent>();
        var gameTimePool = world.GetPool<GameTimeComponent>();
        var businessLevelPool = world.GetPool<BusinessLevelComponent>();
        var businessUpgradesPool = world.GetPool<BusinessUpgradesComponent>();
        var incomeCyclePool = world.GetPool<IncomeCycleComponent>();
        
        var playerCurrencyFilter = world.Filter<PlayerCurrencyComponent>().End();
        var gameTimeFilter = world.Filter<GameTimeComponent>().End();
        var businessLevelFilter = world.Filter<BusinessLevelComponent>().End();
        
        var businessEntities = new List<int>();

        foreach (var entity in businessLevelFilter)
        {
            businessEntities.Add(entity);
        }

        if (businessEntities.Count < saveData.BusinessSaveData.Count)
        {
            UnityEngine.Debug.LogError("[PlayerPrefsSaveService]: Something went very wrong. (incompatibility businessEntities.count vs savedData.BusinessSaveData.Count)");
            return false;
        }
        
        foreach (var entity in playerCurrencyFilter)
        {
            if (decimal.TryParse(saveData.PlayerBalance, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var balance))
            {
                ref var playerCurrencyComponent = ref playerCurrencyPool.Get(entity);
                playerCurrencyComponent.CurrentBalance = balance;
            }

            break;
        }
        
        foreach (var entity in gameTimeFilter)
        {
            ref var gameTimeComponent = ref gameTimePool.Get(entity);
            gameTimeComponent.CurrentTime = saveData.GameTime;
            break;
        }
        
        for (var i = 0; i < saveData.BusinessSaveData.Count; i++)
        {
            var businessEntity = businessEntities[i];
            var businessData = saveData.BusinessSaveData[i];
            
            ref var businessLevelComponent = ref businessLevelPool.Get(businessEntity);
            ref var businessUpgradesComponent = ref businessUpgradesPool.Get(businessEntity);
            ref var incomeCycleComponent = ref incomeCyclePool.Get(businessEntity);
            
            var configReferenceFilter = world.Filter<ConfigReferenceComponent>().End();
            var configReferencePool = world.GetPool<ConfigReferenceComponent>();
        
            var configEntity = -1;
            foreach (var entity in configReferenceFilter)
            {
                configEntity = entity;
                break;
            }
            
            var configReferenceComponent = configReferencePool.Get(configEntity);
            var gameConfig = configReferenceComponent.GameConfig;

            businessLevelComponent.Level = (uint)Mathf.Max(0, businessData.Level);
            businessUpgradesComponent.UpgradesMask = (byte)Mathf.Clamp(businessData.UpgradesMask, 0, 255);

            for (var index = 0; index < gameConfig.Businesses[i].BusinessUpgradeConfig.Length; index++)
            {
                if (!businessUpgradesComponent.HasUpgrade(index))
                {
                    continue;
                }
                
                var upgradeConfig = gameConfig.Businesses[i].BusinessUpgradeConfig[index];
                
                businessUpgradesComponent.Multiplier += upgradeConfig.Multiplier;
            }

            incomeCycleComponent.CycleStartTime = businessData.CycleStartTime;
            incomeCycleComponent.NextIncomeTime = businessData.NextIncomeTime;
            incomeCycleComponent.FullCycleTime = businessData.FullCycleTime;
        }

#if UNITY_EDITOR
        UnityEngine.Debug.Log($"[PlayerPrefsSaveService]: LOAD: [{json}]");
#endif
        return true;
    }
}