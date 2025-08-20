using Leopotam.EcsLite;

public sealed class GameTimeSystem : IEcsInitSystem, IEcsRunSystem
{
    private int _timeEntity;

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        _timeEntity = world.NewEntity();
        ref var gameTimeComponent = ref world.GetPool<GameTimeComponent>().Add(_timeEntity);
        gameTimeComponent.CurrentTime = UnityEngine.Time.timeAsDouble; // 0?
    }

    public void Run(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        ref var gameTimeComponent = ref world.GetPool<GameTimeComponent>().Get(_timeEntity);
        gameTimeComponent.CurrentTime = UnityEngine.Time.timeAsDouble;
    }
}