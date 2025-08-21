using Leopotam.EcsLite;

public sealed class GameTimeSystem : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _timeFilter;
    private EcsPool<GameTimeComponent> _timePool;

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var timeEntity = world.NewEntity();
        
        _timePool = world.GetPool<GameTimeComponent>();
        
        ref var gameTime = ref _timePool.Add(timeEntity);
        gameTime.CurrentTime = 0f;
        
        _timeFilter = world.Filter<GameTimeComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _timeFilter)
        {
            ref var gameTime = ref _timePool.Get(entity);

            gameTime.CurrentTime = UnityEngine.Time.time;
        }
    }
}