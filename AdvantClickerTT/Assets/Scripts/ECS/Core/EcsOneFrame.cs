using Leopotam.EcsLite;

public sealed class EcsOneFrame<T> : IEcsInitSystem, IEcsRunSystem where T : struct
{
    private EcsPool<T> _pool;
    private EcsFilter _filter;

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        _pool = world.GetPool<T>();
        _filter = world.Filter<T>().End();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var e in _filter)
        {
            _pool.Del(e);
        }
    }
}