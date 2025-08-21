using Leopotam.EcsLite;

public sealed class ProcessBusinessLevelUpSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsWorld _world;

    private EcsPool<BusinessLevelUpEventComponent> _businessLevelUpEventPool;
    private EcsPool<BusinessViewReferenceComponent> _businessViewReferencePool;
    private EcsPool<BusinessLevelComponent> _businessLevelPool;
    private EcsPool<PlayerCurrencyComponent> _playerCurrencyPool;
    private EcsPool<BusinessConfigReferenceComponent> _businessConfigReferencePool;

    private EcsFilter _requestedFilter;
    private EcsFilter _playerCurrencyFilter;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();

        _businessLevelUpEventPool = _world.GetPool<BusinessLevelUpEventComponent>();
        _businessViewReferencePool = _world.GetPool<BusinessViewReferenceComponent>();
        _businessLevelPool = _world.GetPool<BusinessLevelComponent>();
        _playerCurrencyPool = _world.GetPool<PlayerCurrencyComponent>();
        _businessConfigReferencePool = _world.GetPool<BusinessConfigReferenceComponent>();

        _requestedFilter = _world.Filter<BusinessLevelUpEventComponent>()
            .Inc<BusinessConfigReferenceComponent>()
            .Inc<BusinessViewReferenceComponent>()
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
            var businessConfigReference = _businessConfigReferencePool.Get(entity);
            ref var viewRef = ref _businessViewReferencePool.Get(entity);
            ref var level = ref _businessLevelPool.Get(entity);

            var price = businessConfigReference.BaseCost * (level.Level + 1);
            if (currency.CurrentBalance < price)
            {
                UnityEngine.Debug.LogError($"An attempt to levelup when currency < price!");
                continue;
            }

            currency.CurrentBalance -= price;
            level.Level += 1;

            viewRef.View.LevelText.text = $"Lvl: {level.Level}";
            viewRef.View.IncomeText.text = $"Income: {businessConfigReference.BaseIncome * level.Level}$";
            viewRef.View.ButtonText.text = $"Level Up\nPrice: {price + businessConfigReference.BaseCost}";
        }
    }
}