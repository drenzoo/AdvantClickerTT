using System;
using Leopotam.EcsLite;

public sealed class ProgressTickSystem : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var timeFilter = world.Filter<GameTimeComponent>().End();
        var timePool = world.GetPool<GameTimeComponent>();

        double currentTime = 0;
        foreach (var t in timeFilter)
        {
            currentTime = timePool.Get(t).CurrentTime;
            break;
        }

        var activeFilter = world.Filter<BusinessConfigReferenceComponent>()
            .Inc<BusinessLevelComponent>()
            .Inc<IncomeCycleComponent>()
            .End();

        var levelPool = world.GetPool<BusinessLevelComponent>();
        var configPool = world.GetPool<BusinessConfigReferenceComponent>();
        var cyclePool = world.GetPool<IncomeCycleComponent>();

        var balancePool = world.GetPool<PlayerCurrencyComponent>();
        var balanceEntity = -1;
        var balanceFilter = world.Filter<PlayerCurrencyComponent>().End();
        foreach (var be in balanceFilter)
        {
            balanceEntity = be;
            break;
        }

        ref var balance = ref balancePool.Get(balanceEntity);

        foreach (var e in activeFilter)
        {
            ref var level = ref levelPool.Get(e);
            ref var config = ref configPool.Get(e);
            
            if (level.Level == 0)
            {
                UnityEngine.Debug.Log($"Skip: #{config.BusinessId}");
                continue;
            }
            
            ref var cycle = ref cyclePool.Get(e);

            var delay = config.BaseDelaySeconds;
            cycle.FullCycleTime = delay;

            if (currentTime < cycle.NextIncomeTime)
            {
                continue;
            }

            var overtime = currentTime - cycle.NextIncomeTime;
            var cyclesCompleted = 1 + (int)Math.Floor(overtime / delay);

            // ref var upgradesImpact = ref pool.Get(e);
            var incomePerCycle = level.Level * config.BaseIncome;// * upgradesImpact.IncomeMultiplier;
            var income = Math.Floor(incomePerCycle) * cyclesCompleted;

            balance.CurrentBalance += income;
            
            UnityEngine.Debug.LogError($"{balance.CurrentBalance}");

            double shift = cyclesCompleted * delay;
            cycle.CycleStartTime += shift;
            cycle.NextIncomeTime += shift;
        }
    }
}