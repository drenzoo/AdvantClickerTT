using Leopotam.EcsLite;
using UnityEngine;

public sealed class MainScreenUISystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
{
    private UIResourcesConfig _uiResourcesConfig;
    private Transform _uiRoot;
    private MoneyFormatter _moneyFormatter;

    private EcsWorld _world;
    private EcsPool<MainScreenViewReferenceComponent> _mainScreenViewPool;
    private EcsPool<PlayerCurrencyComponent> _currencyPool;
    private EcsFilter _mainViewFilter;
    private EcsFilter _currencyFilter;

    public MainScreenUISystem(UIResourcesConfig uiResourcesConfig, Transform uiRoot, MoneyFormatter moneyFormatter)
    {
        _uiResourcesConfig = uiResourcesConfig;
        _uiRoot = uiRoot;
        _moneyFormatter = moneyFormatter;
    }

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _mainScreenViewPool = _world.GetPool<MainScreenViewReferenceComponent>();
        _currencyPool = _world.GetPool<PlayerCurrencyComponent>();

        var view = GameObject.Instantiate(_uiResourcesConfig.MainScreenViewPrefab, _uiRoot);
        var mainScreenEntity = _world.NewEntity();

        ref var mainScreenViewReferenceComponent = ref _mainScreenViewPool.Add(mainScreenEntity);
        mainScreenViewReferenceComponent.MainScreenViewReference = view;

        _mainViewFilter = _world.Filter<MainScreenViewReferenceComponent>().End();
        _currencyFilter   = _world.Filter<PlayerCurrencyComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        var currencyEntity = -1;
        foreach (var e in _currencyFilter)
        {
            currencyEntity = e; break;
        }
        
        ref var currency = ref _currencyPool.Get(currencyEntity);

        foreach (var e in _mainViewFilter)
        {
            ref var mainScreenViewReferenceComponent = ref _mainScreenViewPool.Get(e);
            mainScreenViewReferenceComponent.MainScreenViewReference.HeaderText.text = _moneyFormatter.Format(currency.CurrentBalance); // <-- Error nullref
        }
    }

    public void Destroy(IEcsSystems systems)
    {
        foreach (var entity in _mainViewFilter)
        {
            ref var mainScreenViewReferenceComponent = ref _mainScreenViewPool.Get(entity);
            if (mainScreenViewReferenceComponent.MainScreenViewReference != null)
            {
                GameObject.Destroy(mainScreenViewReferenceComponent.MainScreenViewReference.gameObject);
            }

            _mainScreenViewPool.Del(entity);
            _world.DelEntity(entity);
        }
    }
}