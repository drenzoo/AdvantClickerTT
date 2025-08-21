using System;
using Leopotam.EcsLite;

public sealed class ProgressTickSystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var gameTimeFiler = world.Filter<GameTimeComponent>().End();
        var gameTimePool = world.GetPool<GameTimeComponent>();

        double currentTime = 0;
        foreach (var t in gameTimeFiler)
        {
            currentTime = gameTimePool.Get(t).CurrentTime;
            break;
        }

        var activeFilter = world.Filter<BusinessConfigReferenceComponent>()
            .Inc<BusinessLevelComponent>()
            .Inc<IncomeCycleComponent>()
            .End();

        var businessLevelPool = world.GetPool<BusinessLevelComponent>();
        var businessConfigReferencePool = world.GetPool<BusinessConfigReferenceComponent>();
        var incomeCyclePool = world.GetPool<IncomeCycleComponent>();
        var playerCurrencyPool = world.GetPool<PlayerCurrencyComponent>();
        var businessUpgradesPool = world.GetPool<BusinessUpgradesComponent>();
        
        var balanceEntity = -1;
        var balanceFilter = world.Filter<PlayerCurrencyComponent>().End();
        foreach (var be in balanceFilter)
        {
            balanceEntity = be;
            break;
        }

        ref var playerCurrencyComponent = ref playerCurrencyPool.Get(balanceEntity);

        foreach (var entity in activeFilter)
        {
            ref var businessLevelComponent = ref businessLevelPool.Get(entity);
            ref var businessConfigReferenceComponent = ref businessConfigReferencePool.Get(entity);
            ref var businessUpgradesComponent = ref businessUpgradesPool.Get(entity);
            
            if (businessLevelComponent.Level == 0)
            {
                // UnityEngine.Debug.Log($"Skip: #{businessConfigReferenceComponent.BusinessId}");
                continue;
            }
            
            ref var incomeCycleComponent = ref incomeCyclePool.Get(entity);
            var delay = businessConfigReferenceComponent.BaseDelaySeconds;
            
            incomeCycleComponent.FullCycleTime = delay;

            if (currentTime < incomeCycleComponent.NextIncomeTime)
            {
                continue;
            }

            var overtime = currentTime - incomeCycleComponent.NextIncomeTime;
            var cyclesCompleted = 1 + (int)Math.Floor(overtime / delay);

            var incomePerCycle = businessLevelComponent.Level * businessConfigReferenceComponent.BaseIncome * (decimal)businessUpgradesComponent.Multiplier;
            var income = incomePerCycle * cyclesCompleted;

            UnityEngine.Debug.LogError($"{playerCurrencyComponent.CurrentBalance} + {income}");
            
            playerCurrencyComponent.CurrentBalance += income;

            var shift = cyclesCompleted * delay;
            incomeCycleComponent.CycleStartTime += shift;
            incomeCycleComponent.NextIncomeTime += shift;
        }
    }
}