using Leopotam.EcsLite;
using UnityEngine;

public sealed class BusinessesUISystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private readonly UIResourcesConfig _uiResourcesConfig;
    private readonly MoneyFormatter _moneyFormatter;

    private EcsWorld _world;

    private EcsPool<MainScreenViewReferenceComponent> _mainScreenViewReferencePool;
    private EcsPool<BusinessViewReferenceComponent> _businessViewReferencePool;
    private EcsPool<BusinessConfigReferenceComponent> _businessConfigReferencePool;
    private EcsPool<BusinessUpgradeEventComponent> _businessUpgradeRequestPool;
    private EcsPool<BusinessLevelUpEventComponent> _businessLevelUpRequestPool;
    private EcsPool<BusinessUpgradeButtonViewReferenceComponent> _businessUpgradeButtonViewReferencePool;
    private EcsPool<IncomeCycleComponent> _incomeCyclePool;
    private EcsPool<BusinessLevelComponent> _businessLevelPool;
    private EcsPool<GameTimeComponent> _gameTimePool;
    private EcsPool<PlayerCurrencyComponent> _playerCurrencyPool;
    private EcsPool<ConfigReferenceComponent> _configReferencePool;

    private EcsFilter _mainScreenViewReferenceFilter;
    private EcsFilter _businessConfigReferenceFilter;
    private EcsFilter _businessViewWithConfigFilter;
    private EcsFilter _gameTimeFilter;
    private EcsFilter _playerCurrencyFilter;
    private EcsFilter _configReferenceFilter;
    private EcsFilter _businessUpgradeButtonViewReferenceFilter;

    public BusinessesUISystem(UIResourcesConfig uiResourcesConfigResourcesConfig, MoneyFormatter moneyFormatter)
    {
        _uiResourcesConfig = uiResourcesConfigResourcesConfig;
        _moneyFormatter = moneyFormatter;
    }

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();

        _mainScreenViewReferencePool = _world.GetPool<MainScreenViewReferenceComponent>();
        _businessViewReferencePool = _world.GetPool<BusinessViewReferenceComponent>();
        _businessConfigReferencePool = _world.GetPool<BusinessConfigReferenceComponent>();
        _businessLevelUpRequestPool = _world.GetPool<BusinessLevelUpEventComponent>();
        _businessUpgradeRequestPool = _world.GetPool<BusinessUpgradeEventComponent>();
        _businessUpgradeButtonViewReferencePool = _world.GetPool<BusinessUpgradeButtonViewReferenceComponent>();
        _incomeCyclePool = _world.GetPool<IncomeCycleComponent>();
        _businessLevelPool = _world.GetPool<BusinessLevelComponent>();
        _gameTimePool = _world.GetPool<GameTimeComponent>();
        _playerCurrencyPool = _world.GetPool<PlayerCurrencyComponent>();
        _configReferencePool = _world.GetPool<ConfigReferenceComponent>();

        _mainScreenViewReferenceFilter = _world.Filter<MainScreenViewReferenceComponent>().End();
        _businessConfigReferenceFilter = _world.Filter<BusinessConfigReferenceComponent>().End();
        _businessViewWithConfigFilter = _world.Filter<BusinessViewReferenceComponent>().Inc<BusinessConfigReferenceComponent>().End();
        _gameTimeFilter = _world.Filter<GameTimeComponent>().End();
        _playerCurrencyFilter = _world.Filter<PlayerCurrencyComponent>().End();
        _configReferenceFilter = _world.Filter<ConfigReferenceComponent>().End();
        _businessUpgradeButtonViewReferenceFilter = _world.Filter<BusinessUpgradeButtonViewReferenceComponent>().End();

        var mainScreenEntity = -1;
        foreach (var entity in _mainScreenViewReferenceFilter)
        {
            mainScreenEntity = entity;
            break;
        }

        var mainScreenView = _mainScreenViewReferencePool.Get(mainScreenEntity).MainScreenViewReference;
        var businessLayer = mainScreenView.BusinessLayer;

        var configEntity = -1;
        foreach (var entity in _configReferenceFilter)
        {
            configEntity = entity;
            break;
        }

        ref var configReferenceComponent = ref _configReferencePool.Get(configEntity);
        var nameConfig = configReferenceComponent.NameConfig;
        var gameConfig = configReferenceComponent.GameConfig;

        foreach (var businessEntity in _businessConfigReferenceFilter)
        {
            var businessConfigReference = _businessConfigReferencePool.Get(businessEntity);
            var businessLevel = _businessLevelPool.Get(businessEntity).Level;

            var businessView = Object.Instantiate(_uiResourcesConfig.BusinessViewPrefab, businessLayer);
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Add(businessEntity);
            businessViewReferenceComponent.View = businessView;

            businessView.HeaderText.text = nameConfig.GetBusinessName(businessConfigReference.BusinessId);
            businessView.LevelText.text = $"Lvl: {businessLevel}";
            businessView.ButtonText.text = $"Level up\nPrice: {businessConfigReference.BaseCost * (businessLevel + 1)}";
            businessView.IncomeText.text = $"Income: {_moneyFormatter.Format(businessConfigReference.BaseIncome * businessLevel)}$";

            businessView.LevelUpButton.onClick.AddListener(() =>
            {
                if (!_businessLevelUpRequestPool.Has(businessEntity))
                {
                    _businessLevelUpRequestPool.Add(businessEntity);
                }
            });

            var businessUpgradeConfig = gameConfig.Businesses[businessConfigReference.BusinessId].BusinessUpgradeConfig;
            for (var upgradeIndex = 0; upgradeIndex < businessUpgradeConfig.Length; upgradeIndex++)
            {
                var upgradeButtonViewReference = Object.Instantiate(_uiResourcesConfig.UpgradeButtonViewPrefab, businessView.UnlocksLayer);
                upgradeButtonViewReference.ButtonText.text = nameConfig.GetUpgradeName(businessConfigReference.BusinessId, upgradeIndex);

                var btnEntity = _world.NewEntity();
                ref var businessUpgradeButtonViewReference = ref _businessUpgradeButtonViewReferencePool.Add(btnEntity);
                businessUpgradeButtonViewReference.View = upgradeButtonViewReference;
                businessUpgradeButtonViewReference.UpgradeIndex = upgradeIndex;
                businessUpgradeButtonViewReference.Price = businessUpgradeConfig[upgradeIndex].Price;

                var capturedIndex = upgradeIndex;
                upgradeButtonViewReference.UpgradeButton.onClick.AddListener(() =>
                {
                    ref var businessUpgradeRequest = ref _businessUpgradeRequestPool.Add(businessEntity);
                    businessUpgradeRequest.UpgradeIndex = capturedIndex;
                });
            }
        }
    }

    public void Run(IEcsSystems systems)
    {
        var timeEntity = -1;
        foreach (var e in _gameTimeFilter)
        {
            timeEntity = e;
            break;
        }

        ref var gameTimeComponent = ref _gameTimePool.Get(timeEntity);
        var currentTime = gameTimeComponent.CurrentTime;

        var currencyEntity = -1;
        foreach (var e in _playerCurrencyFilter)
        {
            currencyEntity = e;
            break;
        }

        ref var currency = ref _playerCurrencyPool.Get(currencyEntity);

        foreach (var entity in _businessViewWithConfigFilter)
        {
            var businessConfigReference = _businessConfigReferencePool.Get(entity);
            var businessLevel = _businessLevelPool.Get(entity).Level;
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Get(entity);
            ref var incomeCycleComponent = ref _incomeCyclePool.Get(entity);

            var remaining = incomeCycleComponent.NextIncomeTime - currentTime;
            var progress = 1f - (remaining / incomeCycleComponent.FullCycleTime);
            if (progress < 0f)
            {
                progress = 0f;
            }
            else if (progress > 1f)
            {
                progress = 1f;
            }

            businessViewReferenceComponent.View.Slider.value = progress;
            businessViewReferenceComponent.View.LevelUpButton.interactable = currency.CurrentBalance >= (businessConfigReference.BaseCost * (businessLevel + 1));
        }

        foreach (var entity in _businessUpgradeButtonViewReferenceFilter)
        {
            ref var businessUpgradeButtonViewReferenceComponent = ref _businessUpgradeButtonViewReferencePool.Get(entity);
            businessUpgradeButtonViewReferenceComponent.View.UpgradeButton.interactable = currency.CurrentBalance >= businessUpgradeButtonViewReferenceComponent.Price;
        }
    }

    public void Destroy(IEcsSystems systems)
    {
        foreach (var entity in _businessViewWithConfigFilter)
        {
            _businessViewReferencePool.Del(entity);
        }

        foreach (var entity in _businessUpgradeButtonViewReferenceFilter)
        {
            _businessUpgradeButtonViewReferencePool.Del(entity);
            _world.DelEntity(entity);
        }
    }
}