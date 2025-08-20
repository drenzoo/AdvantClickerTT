using Leopotam.EcsLite;

public sealed class GameStartSystem : IEcsInitSystem
{
    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        
        var entity = world.NewEntity();
        
        ref var playerCurrencyComponent = ref world.GetPool<PlayerCurrencyComponent>().Add(entity);
        
        playerCurrencyComponent.CurrentBalance = 0;
    }
}