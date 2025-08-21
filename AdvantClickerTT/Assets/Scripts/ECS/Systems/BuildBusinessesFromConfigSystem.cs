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
        
        foreach (var businessConfig in gameConfig.Businesses)
        {
            var entity = world.NewEntity();
            
            ref var businessConfigReferenceComponent = ref businessConfigReferencePool.Add(entity);
            ref var levelComponent = ref businessLevelPool.Add(entity);
            ref var incomeCycleComponent = ref incomeCyclePool.Add(entity);
            
            businessConfigReferenceComponent.BusinessId = businessConfig.Id;
            businessConfigReferenceComponent.BaseCost = businessConfig.BaseCost;
            businessConfigReferenceComponent.BaseIncome = businessConfig.BaseIncome;
            businessConfigReferenceComponent.BaseDelaySeconds = businessConfig.DelaySeconds;

            levelComponent.Level = (businessConfig.Id == FirstBusinessesID) ? 1u : 0u;

            incomeCycleComponent.FullCycleTime = businessConfigReferenceComponent.BaseDelaySeconds;
            incomeCycleComponent.CycleStartTime = currentTime;
            incomeCycleComponent.NextIncomeTime = currentTime + businessConfigReferenceComponent.BaseDelaySeconds;

            // ref var mask = ref upMaskPool.Add(e);
            // mask.Bits = 0;
        }
    }
}