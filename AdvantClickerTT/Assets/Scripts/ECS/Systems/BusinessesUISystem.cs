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
    private EcsPool<UpdateBusinessUIEventComponent> _updateBusinessUIEventPool;
    private EcsPool<BusinessUpgradeButtonViewReferenceComponent> _businessUpgradeButtonViewReferencePool;
    private EcsPool<BusinessUpgradeButtonDataComponent> _businessUpgradeButtonDataPool;
    private EcsPool<IncomeCycleComponent> _incomeCyclePool;
    private EcsPool<BusinessLevelComponent> _businessLevelPool;
    private EcsPool<GameTimeComponent> _gameTimePool;
    private EcsPool<PlayerCurrencyComponent> _playerCurrencyPool;
    private EcsPool<ConfigReferenceComponent> _configReferencePool;
    private EcsPool<OwnerComponent> _ownerPool;
    private EcsPool<BusinessUpgradesComponent> _businessUpgradesPool;

    private EcsFilter _mainScreenViewReferenceFilter;
    private EcsFilter _businessConfigReferenceFilter;
    private EcsFilter _businessViewWithConfigFilter;
    private EcsFilter _gameTimeFilter;
    private EcsFilter _playerCurrencyFilter;
    private EcsFilter _configReferenceFilter;
    private EcsFilter _businessUpgradeButtonViewReferenceFilter;
    private EcsFilter _upgradeButtonsFilter;
    private EcsFilter _updateBusinessUIEventFilter;

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
        _updateBusinessUIEventPool = _world.GetPool<UpdateBusinessUIEventComponent>();
        _businessUpgradeButtonViewReferencePool = _world.GetPool<BusinessUpgradeButtonViewReferenceComponent>();
        _businessUpgradeButtonDataPool = _world.GetPool<BusinessUpgradeButtonDataComponent>();
        _businessUpgradesPool = _world.GetPool<BusinessUpgradesComponent>();
        _ownerPool = _world.GetPool<OwnerComponent>();
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
        _upgradeButtonsFilter = _world.Filter<BusinessUpgradeButtonViewReferenceComponent>().Inc<OwnerComponent>().End();
        _updateBusinessUIEventFilter = _world.Filter<UpdateBusinessUIEventComponent>().End();

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
        var gameConfig = configReferenceComponent.GameConfig;

        foreach (var businessEntity in _businessConfigReferenceFilter)
        {
            var businessConfigReferenceComponent = _businessConfigReferencePool.Get(businessEntity);
            var businessView = Object.Instantiate(_uiResourcesConfig.BusinessViewPrefab, businessLayer);
            
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Add(businessEntity);
            
            businessViewReferenceComponent.View = businessView;

            businessView.LevelUpButton.onClick.AddListener(() =>
            {
                if (!_businessLevelUpRequestPool.Has(businessEntity))
                {
                    _businessLevelUpRequestPool.Add(businessEntity);
                }
            });

            var businessUpgradeConfig = gameConfig.Businesses[businessConfigReferenceComponent.BusinessId].BusinessUpgradeConfig;
            for (var upgradeIndex = 0; upgradeIndex < businessUpgradeConfig.Length; upgradeIndex++)
            {
                var upgradeButtonViewReference = Object.Instantiate(_uiResourcesConfig.UpgradeButtonViewPrefab, businessView.UnlocksLayer);
                var upgradeEntity = _world.NewEntity();
                
                ref var businessUpgradeButtonViewReferenceComponent = ref _businessUpgradeButtonViewReferencePool.Add(upgradeEntity);
                ref var businessUpgradeButtonDataComponent = ref _businessUpgradeButtonDataPool.Add(upgradeEntity);
                ref var ownerComponent = ref _ownerPool.Add(upgradeEntity);

                ownerComponent.Entity = businessEntity;
                businessUpgradeButtonViewReferenceComponent.View = upgradeButtonViewReference;
                businessUpgradeButtonDataComponent.Price = businessUpgradeConfig[upgradeIndex].Price;
                businessUpgradeButtonDataComponent.UpgradeIndex = upgradeIndex;

                var capturedIndex = upgradeIndex;
                upgradeButtonViewReference.UpgradeButton.onClick.AddListener(() =>
                {
                    ref var businessUpgradeRequest = ref _businessUpgradeRequestPool.Add(businessEntity);
                    businessUpgradeRequest.UpgradeIndex = capturedIndex;
                });
            }
        }

        UpdateBusinessUI();
    }

    public void Run(IEcsSystems systems)
    {
        var timeEntity = -1;
        foreach (var entity in _gameTimeFilter)
        {
            timeEntity = entity;
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
            
            businessViewReferenceComponent.View.LevelUpButton.interactable = currency.CurrentBalance >= (businessConfigReference.BaseCost * (businessLevel + 1));
            
            if (businessLevel < 1)
            {
                continue;
            }
            
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
        }

        foreach (var entity in _businessUpgradeButtonViewReferenceFilter)
        {
            ref var businessUpgradeButtonViewReferenceComponent = ref _businessUpgradeButtonViewReferencePool.Get(entity);
            var ownerEntity = _ownerPool.Get(entity).Entity;
            ref var upgrades = ref _businessUpgradesPool.Get(ownerEntity);
            var businessUpgradeButtonDataComponent = _businessUpgradeButtonDataPool.Get(entity);
            var isBought = upgrades.HasUpgrade(businessUpgradeButtonDataComponent.UpgradeIndex);

            if (isBought)
            {
                continue;
            }
            
            businessUpgradeButtonViewReferenceComponent.View.UpgradeButton.interactable = currency.CurrentBalance >= businessUpgradeButtonDataComponent.Price;
        }
        
        foreach (var _ in _updateBusinessUIEventFilter)
        {
            UpdateBusinessUI();
            break;
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

    private void UpdateBusinessUI()
    {
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
            var businessConfigReferenceComponent = _businessConfigReferencePool.Get(businessEntity);
            var businessLevel = _businessLevelPool.Get(businessEntity).Level;
            var businessUpgradesComponent = _businessUpgradesPool.Get(businessEntity);
            
            ref var businessViewReferenceComponent = ref _businessViewReferencePool.Get(businessEntity);

            var income = _moneyFormatter.Format(businessConfigReferenceComponent.BaseIncome * businessLevel * (decimal)businessUpgradesComponent.Multiplier);

            businessViewReferenceComponent.View.Slider.value = 0f;
            businessViewReferenceComponent.View.HeaderText.text = nameConfig.GetBusinessName(businessConfigReferenceComponent.BusinessId);
            businessViewReferenceComponent.View.LevelText.text = $"Lvl: {businessLevel}";
            businessViewReferenceComponent.View.ButtonText.text = $"Level up\nPrice: {businessConfigReferenceComponent.BaseCost * (businessLevel + 1)}";
            businessViewReferenceComponent.View.IncomeText.text = $"Income: {income}$";

            var businessUpgradeConfig = gameConfig.Businesses[businessConfigReferenceComponent.BusinessId].BusinessUpgradeConfig;
            
            foreach (var upgradeButtonEntity in _upgradeButtonsFilter)
            {
                var ownerEntity = _ownerPool.Get(upgradeButtonEntity).Entity;
                if (ownerEntity != businessEntity)
                {
                    continue;
                }

                ref var businessUpgradeButtonViewReferenceComponent = ref _businessUpgradeButtonViewReferencePool.Get(upgradeButtonEntity);
                var businessUpgradeButtonDataComponent = _businessUpgradeButtonDataPool.Get(upgradeButtonEntity);
                var additionalMultiplier = gameConfig.Businesses[businessConfigReferenceComponent.BusinessId].BusinessUpgradeConfig[businessUpgradeButtonDataComponent.UpgradeIndex].Multiplier;
                var hasUpgrade = businessUpgradesComponent.HasUpgrade(businessUpgradeButtonDataComponent.UpgradeIndex);
                var lastRowString = hasUpgrade ? "Purchased!" : $"Price: {businessUpgradeConfig[businessUpgradeButtonDataComponent.UpgradeIndex].Price}$";

                businessUpgradeButtonViewReferenceComponent.View.UpgradeButton.interactable = !hasUpgrade;
                businessUpgradeButtonViewReferenceComponent.View.ButtonText.text = $"{nameConfig.GetUpgradeName(businessConfigReferenceComponent.BusinessId, businessUpgradeButtonDataComponent.UpgradeIndex)}\n" +
                                                                                   $"Income +{additionalMultiplier * 100f}%\n" + 
                                                                                   lastRowString;
            }
        }
    }
}