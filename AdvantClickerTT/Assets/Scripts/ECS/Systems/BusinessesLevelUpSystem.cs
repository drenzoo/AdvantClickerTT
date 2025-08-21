using Leopotam.EcsLite;

public sealed class ProcessBusinessLevelUpSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly MoneyFormatter _moneyFormatter;
    
    private EcsWorld _world;

    private EcsPool<BusinessLevelUpEventComponent> _businessLevelUpEventPool;
    private EcsPool<BusinessViewReferenceComponent> _businessViewReferencePool;
    private EcsPool<BusinessLevelComponent> _businessLevelPool;
    private EcsPool<PlayerCurrencyComponent> _playerCurrencyPool;
    private EcsPool<BusinessConfigReferenceComponent> _businessConfigReferencePool;
    private EcsPool<BusinessUpgradesComponent> _businessUpgradesPool;
    private EcsPool<IncomeCycleComponent> _incomeCyclePool;

    private EcsFilter _requestedFilter;
    private EcsFilter _playerCurrencyFilter;

    public ProcessBusinessLevelUpSystem(MoneyFormatter moneyFormatter)
    {
        _moneyFormatter = moneyFormatter;
    }

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();

        _businessLevelUpEventPool = _world.GetPool<BusinessLevelUpEventComponent>();
        _businessViewReferencePool = _world.GetPool<BusinessViewReferenceComponent>();
        _businessLevelPool = _world.GetPool<BusinessLevelComponent>();
        _playerCurrencyPool = _world.GetPool<PlayerCurrencyComponent>();
        _businessConfigReferencePool = _world.GetPool<BusinessConfigReferenceComponent>();
        _businessUpgradesPool = _world.GetPool<BusinessUpgradesComponent>();
        _incomeCyclePool = _world.GetPool<IncomeCycleComponent>();

        _requestedFilter = _world.Filter<BusinessLevelUpEventComponent>()
            .Inc<BusinessConfigReferenceComponent>()
            .Inc<BusinessViewReferenceComponent>()
            .Inc<BusinessUpgradesComponent>()
            .Inc<BusinessLevelComponent>().End();

        _playerCurrencyFilter = _world.Filter<PlayerCurrencyComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        var currencyEntity = -1;
        foreach (var entity in _playerCurrencyFilter)
        {
            currencyEntity = entity;
            break;
        }

        ref var currency = ref _playerCurrencyPool.Get(currencyEntity);

        foreach (var entity in _requestedFilter)
        {
            var businessConfigReferenceComponent = _businessConfigReferencePool.Get(entity);
            var businessUpgradesComponent = _businessUpgradesPool.Get(entity);
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Get(entity);
            ref var businessLevelComponent = ref _businessLevelPool.Get(entity);
            ref var incomeCycleComponent = ref _incomeCyclePool.Get(entity);

            var price = businessConfigReferenceComponent.BaseCost * (businessLevelComponent.Level + 1);
            if (currency.CurrentBalance < price)
            {
                UnityEngine.Debug.LogError($"An attempt to levelup when currency < price!");
                continue;
            }

            if (businessLevelComponent.Level == 0)
            {
                var gameTimeFilter = _world.Filter<GameTimeComponent>().End();
                var gameTimePool = _world.GetPool<GameTimeComponent>();
                
                float currentTime = 0;
                foreach (var t in gameTimeFilter)
                {
                    currentTime = gameTimePool.Get(t).CurrentTime;
                    break;
                }

                incomeCycleComponent.CycleStartTime = currentTime;
                incomeCycleComponent.NextIncomeTime = currentTime + businessConfigReferenceComponent.BaseDelaySeconds;
            }

            currency.CurrentBalance -= price;
            businessLevelComponent.Level += 1;

            var income = _moneyFormatter.Format(businessConfigReferenceComponent.BaseIncome * businessLevelComponent.Level * (decimal)businessUpgradesComponent.Multiplier);

            businessViewReferenceComponent.View.LevelText.text = $"Lvl: {businessLevelComponent.Level}";
            businessViewReferenceComponent.View.IncomeText.text = $"Income: {income}$";
            businessViewReferenceComponent.View.ButtonText.text = $"Level Up\nPrice: {price + businessConfigReferenceComponent.BaseCost}";
        }
    }
}