public interface ISaveService
{
    void Save(Leopotam.EcsLite.EcsWorld world);
    bool TryLoad(Leopotam.EcsLite.EcsWorld world);
}