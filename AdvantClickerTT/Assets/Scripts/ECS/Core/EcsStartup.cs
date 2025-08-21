using System;
using Leopotam.EcsLite;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private NameConfig _nameConfig;
    [SerializeField] private UIResourcesConfig _uiResourcesConfig;
    [SerializeField] private Transform _uiRoot;

    private EcsWorld _world;

    private IEcsSystems _systems;
    private ISaveService _saveService;
    private MoneyFormatter _moneyFormatter;
    
    private int _systemEntity;

    private void Awake()
    {
        _saveService = new PlayerPrefsSaveService();
        _moneyFormatter = new MoneyFormatter();
        
        _world = new EcsWorld();

        _systems = new EcsSystems(_world)
                .Add(new GameStartSystem())
                .Add(new GameTimeSystem())
                .Add(new LoadConfigsSystem(_gameConfig, _nameConfig))
                .Add(new BuildBusinessesFromConfigSystem()) 
                .Add(new ProgressTickSystem())
            
                .Add(new MainScreenUISystem(_uiResourcesConfig, _uiRoot, _moneyFormatter))
                .Add(new BusinessesUISystem(_uiResourcesConfig, _moneyFormatter))
                
                .Add(new ProcessBusinessLevelUpSystem(_moneyFormatter))
                .Add(new BusinessesUpgradeSystem(_moneyFormatter))
                
                .Add(new EcsOneFrame<BusinessLevelUpEventComponent>())
                .Add(new EcsOneFrame<BusinessUpgradeEventComponent>())
                .Add(new EcsOneFrame<UpdateBusinessUIEventComponent>())
            ;
        
        _systems.Init();
        _systemEntity = _world.NewEntity();
        
        if (_saveService.TryLoad(_world))
        {
            RequestBusinessUIUpdate();
        }
    }

    private void RequestBusinessUIUpdate()
    {
        var businessUpgradeRequestPool = _world.GetPool<UpdateBusinessUIEventComponent>();
        businessUpgradeRequestPool.Add(_systemEntity);
    }

    private void Update()
    {
        _systems?.Run();
    }

    private void OnApplicationQuit()
    {
        _saveService?.Save(_world);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            _saveService?.Save(_world);
        }
    }

    private void OnDestroy()
    {
        _systems?.Destroy();
        _world?.Destroy();

        _systems = null;
        _world = null;
    }
}