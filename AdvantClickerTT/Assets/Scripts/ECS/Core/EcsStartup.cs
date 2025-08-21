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
    //private ISaveService _saveService;
    private MoneyFormatter _moneyFormatter;

    private void Awake()
    {
        //_saveService = new JsonSaveService();
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
            ;
        
        _systems.Init();
    }

    private void Update()
    {
        _systems?.Run();
    }

    private void OnApplicationQuit()
    {
        // _saveService.Save(_world);
    }

    private void OnDestroy()
    {
        _systems?.Destroy();
        _world?.Destroy();

        _systems = null;
        _world = null;
    }
}