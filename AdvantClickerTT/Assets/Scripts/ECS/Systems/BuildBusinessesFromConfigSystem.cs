using Leopotam.EcsLite;

public sealed class BuildBusinessesFromConfigSystem : IEcsInitSystem
{
    private const int FirstBusinessesID = 0;
    
    // private readonly UiRoot _uiRoot;

    // public BuildBusinessesFromConfigSystem(UiRoot uiRoot)
    // {
    //     _uiRoot = uiRoot;
    // }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        
        var configFilter = world.Filter<ConfigReferenceComponent>().End();
        var timeFilter = world.Filter<GameTimeComponent>().End();
        
        var globalConfigPool = world.GetPool<ConfigReferenceComponent>();
        var timePool = world.GetPool<GameTimeComponent>();
        
        var configEntity = -1;
        foreach (var e in configFilter)
        {
            configEntity = e;
            break;
        }

        var globalConfig = globalConfigPool.Get(configEntity);
        var gameCfg = globalConfig.GameConfig;
        var nameCfg = globalConfig.NameConfig;

        double now = 0;
        foreach (var t in timeFilter)
        {
            now = timePool.Get(t).CurrentTime;
            break;
        }

        var businessConfigPool = world.GetPool<BusinessConfigReferenceComponent>();
        var levelPool = world.GetPool<BusinessLevelComponent>();
        var cyclePool = world.GetPool<IncomeCycleComponent>();
        //var upMaskPool = world.GetPool<UpgradesMask>();
        //var upEffectPool = world.GetPool<UpgradesEffectCache>();
        
        foreach (var b in gameCfg.Businesses)
        {
            var e = world.NewEntity();
            
            ref var configReferenceComponent = ref businessConfigPool.Add(e);
            ref var level = ref levelPool.Add(e);
            ref var productionCycle = ref cyclePool.Add(e);
            
            configReferenceComponent.BusinessId = b.Id;
            configReferenceComponent.BaseCost = b.BaseCost;
            configReferenceComponent.BaseIncome = b.BaseIncome;
            configReferenceComponent.BaseDelaySeconds = b.DelaySeconds;

            level.Level = (b.Id == FirstBusinessesID) ? 1u : 0u;

            productionCycle.FullCycleTime = configReferenceComponent.BaseDelaySeconds;
            productionCycle.CycleStartTime = now;
            productionCycle.NextIncomeTime = now + configReferenceComponent.BaseDelaySeconds;

            // ref var mask = ref upMaskPool.Add(e);
            // mask.Bits = 0;
            
            // var row = _uiRoot.SpawnBusinessRow();
            // row.Init(id.Value, nameCfg.GetBusinessName(id.Value), nameCfg.GetUpgradeName(id.Value, 1), nameCfg.GetUpgradeName(id.Value, 2));
            // ref var view = ref viewPool.Add(e);
            // view.View = row;

            // world.GetPool<RecomputeEconomyEvent>().Add(world.NewEntity());
        }
    }
}