using Leopotam.EcsLite;

public sealed class BusinessesUpgradeSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly MoneyFormatter _moneyFormatter;

    private EcsWorld _world;

    private EcsPool<BusinessUpgradeEventComponent> _businessUpgradeEventComponent;
    private EcsPool<BusinessViewReferenceComponent> _businessViewReferencePool;
    private EcsPool<BusinessLevelComponent> _businessLevelPool;
    private EcsPool<PlayerCurrencyComponent> _playerCurrencyPool;
    private EcsPool<BusinessConfigReferenceComponent> _businessConfigReferencePool;
    private EcsPool<BusinessUpgradesComponent> _businessUpgradesPool;
    private EcsPool<ConfigReferenceComponent> _configReferencePool;
    private EcsPool<BusinessUpgradeButtonViewReferenceComponent> _businessUpgradeButtonViewReferencePool;
    private EcsPool<BusinessUpgradeButtonDataComponent> _businessUpgradeButtonDataPool;
    private EcsPool<OwnerComponent> _ownerPool;

    private EcsFilter _requestedFilter;
    private EcsFilter _playerCurrencyFilter;
    private EcsFilter _configReferenceFilter;
    private EcsFilter _upgradeButtonsFilter;

    public BusinessesUpgradeSystem(MoneyFormatter moneyFormatter)
    {
        _moneyFormatter = moneyFormatter;
    }

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();

        _businessUpgradeEventComponent = _world.GetPool<BusinessUpgradeEventComponent>();
        _businessViewReferencePool = _world.GetPool<BusinessViewReferenceComponent>();
        _businessLevelPool = _world.GetPool<BusinessLevelComponent>();
        _playerCurrencyPool = _world.GetPool<PlayerCurrencyComponent>();
        _businessConfigReferencePool = _world.GetPool<BusinessConfigReferenceComponent>();
        _businessUpgradesPool = _world.GetPool<BusinessUpgradesComponent>();
        _configReferencePool = _world.GetPool<ConfigReferenceComponent>();
        _businessUpgradeButtonViewReferencePool = _world.GetPool<BusinessUpgradeButtonViewReferenceComponent>();
        _businessUpgradeButtonDataPool = _world.GetPool<BusinessUpgradeButtonDataComponent>();
        _ownerPool = _world.GetPool<OwnerComponent>();

        _requestedFilter = _world.Filter<BusinessUpgradeEventComponent>()
            .Inc<BusinessConfigReferenceComponent>()
            .Inc<BusinessViewReferenceComponent>()
            .Inc<BusinessUpgradesComponent>()
            .Inc<BusinessLevelComponent>()
            .End();

        _playerCurrencyFilter = _world.Filter<PlayerCurrencyComponent>().End();
        _configReferenceFilter = _world.Filter<ConfigReferenceComponent>().End();
        _upgradeButtonsFilter = _world.Filter<BusinessUpgradeButtonViewReferenceComponent>().Inc<OwnerComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        var currencyEntity = -1;
        foreach (var entity in _playerCurrencyFilter)
        {
            currencyEntity = entity;
            break;
        }

        var configEntity = -1;
        foreach (var entity in _configReferenceFilter)
        {
            configEntity = entity;
            break;
        }

        ref var currency = ref _playerCurrencyPool.Get(currencyEntity);
        var gameConfig = _configReferencePool.Get(configEntity).GameConfig;
        var nameConfig = _configReferencePool.Get(configEntity).NameConfig;

        foreach (var entity in _requestedFilter)
        {
            var businessConfigReferenceComponent = _businessConfigReferencePool.Get(entity);
            var businessLevelComponent = _businessLevelPool.Get(entity);
            var businessUpgradeComponent = _businessUpgradeEventComponent.Get(entity);
            ref var businessUpgradesComponent = ref _businessUpgradesPool.Get(entity);
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Get(entity);

            var price = gameConfig.Businesses[businessConfigReferenceComponent.BusinessId].BusinessUpgradeConfig[businessUpgradeComponent.UpgradeIndex].Price;
            if (currency.CurrentBalance < price)
            {
                UnityEngine.Debug.LogError($"An attempt to upgrade when currency < price!");
                continue;
            }

            currency.CurrentBalance -= price;
            var additionalMultiplier = gameConfig.Businesses[businessConfigReferenceComponent.BusinessId].BusinessUpgradeConfig[businessUpgradeComponent.UpgradeIndex].Multiplier;
            businessUpgradesComponent.Multiplier += additionalMultiplier;
            businessUpgradesComponent.AddUpgrade(businessUpgradeComponent.UpgradeIndex);

            foreach (var upgradeButtonEntity in _upgradeButtonsFilter)
            {
                var ownerEntity = _ownerPool.Get(upgradeButtonEntity).Entity;
                if (ownerEntity != entity)
                {
                    continue;
                }

                ref var businessUpgradeButtonViewReferenceComponent = ref _businessUpgradeButtonViewReferencePool.Get(upgradeButtonEntity);
                var businessUpgradeButtonDataComponent = _businessUpgradeButtonDataPool.Get(upgradeButtonEntity);

                if (businessUpgradeButtonDataComponent.UpgradeIndex != businessUpgradeComponent.UpgradeIndex)
                {
                    continue;
                }

                businessUpgradeButtonViewReferenceComponent.View.UpgradeButton.interactable = false;
                businessUpgradeButtonViewReferenceComponent.View.ButtonText.text = $"{nameConfig.GetUpgradeName(businessConfigReferenceComponent.BusinessId, businessUpgradeComponent.UpgradeIndex)}\n" +
                                                                                   $"Income +{additionalMultiplier * 100f}%\n" +
                                                                                   $"Purchased!";
                break;
            }

            var income = _moneyFormatter.Format(businessConfigReferenceComponent.BaseIncome * businessLevelComponent.Level * (decimal)businessUpgradesComponent.Multiplier);

            businessViewReferenceComponent.View.IncomeText.text = $"Income: {income}$";
        }
    }
}