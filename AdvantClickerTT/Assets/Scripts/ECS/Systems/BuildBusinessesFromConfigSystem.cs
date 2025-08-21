using Leopotam.EcsLite;

public sealed class BuildBusinessesFromConfigSystem : IEcsInitSystem
{
    private const int FirstBusinessesID = 0;

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        
        var configReferenceFilter = world.Filter<ConfigReferenceComponent>().End();
        var gameTimeFilter = world.Filter<GameTimeComponent>().End();
        
        var configReferencePool = world.GetPool<ConfigReferenceComponent>();
        var gameTimePool = world.GetPool<GameTimeComponent>();
        
        var configEntity = -1;
        foreach (var e in configReferenceFilter)
        {
            configEntity = e;
            break;
        }

        ref var configReferenceComponent = ref configReferencePool.Get(configEntity);
        var gameConfig = configReferenceComponent.GameConfig;
        var nameConfig = configReferenceComponent.NameConfig;

        float currentTime = 0;
        foreach (var t in gameTimeFilter)
        {
            currentTime = gameTimePool.Get(t).CurrentTime;
            break;
        }

        var businessConfigReferencePool = world.GetPool<BusinessConfigReferenceComponent>();
        var businessLevelPool = world.GetPool<BusinessLevelComponent>();
        var incomeCyclePool = world.GetPool<IncomeCycleComponent>();
        //var upMaskPool = world.GetPool<UpgradesMask>();
        //var upEffectPool = world.GetPool<UpgradesEffectCache>();
        
        foreach (var b in gameConfig.Businesses)
        {
            var e = world.NewEntity();
            
            ref var businessConfigReferenceComponent = ref businessConfigReferencePool.Add(e);
            ref var level = ref businessLevelPool.Add(e);
            ref var productionCycle = ref incomeCyclePool.Add(e);
            
            businessConfigReferenceComponent.BusinessId = b.Id;
            businessConfigReferenceComponent.BaseCost = b.BaseCost;
            businessConfigReferenceComponent.BaseIncome = b.BaseIncome;
            businessConfigReferenceComponent.BaseDelaySeconds = b.DelaySeconds;

            level.Level = (b.Id == FirstBusinessesID) ? 1u : 0u;

            productionCycle.FullCycleTime = businessConfigReferenceComponent.BaseDelaySeconds;
            productionCycle.CycleStartTime = currentTime;
            productionCycle.NextIncomeTime = currentTime + businessConfigReferenceComponent.BaseDelaySeconds;

            // ref var mask = ref upMaskPool.Add(e);
            // mask.Bits = 0;
        }
    }
}