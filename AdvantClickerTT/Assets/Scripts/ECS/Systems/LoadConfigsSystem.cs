using Leopotam.EcsLite;

public sealed class LoadConfigsSystem : IEcsInitSystem
{
    private readonly GameConfig _gameConfig;
    private readonly NameConfig _nameConfig;

    public LoadConfigsSystem(GameConfig gameConfig, NameConfig nameConfig)
    {
        _gameConfig = gameConfig;
        _nameConfig = nameConfig;
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var entity = world.NewEntity();
        var pool = world.GetPool<ConfigReferenceComponent>();

        ref var cfg = ref pool.Add(entity);

        cfg.GameConfig = _gameConfig;
        cfg.NameConfig = _nameConfig;
    }
}