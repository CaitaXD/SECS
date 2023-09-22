using SECS.Systems;

namespace SECS;
public readonly struct Game : ISystem
{
    readonly List<ISystem> _systems;
    public Game()
    {
        _systems = new List<ISystem>();
    }
    public void AddSystem(ISystem system)
    {
        _systems.Add(system);
    }
    public void RemoveSystem(ISystem system)
    {
        _systems.Remove(system);
    }
    public void AddSystem<TSystem>() where TSystem : ISystem, new()
    {
        _systems.Add(new TSystem());
    }
    public void RemoveSystem<TSystem>() where TSystem : ISystem
    {
        _systems.Remove(_systems.First(x => x is TSystem));
    }
    public void ClearSystems()
    {
        _systems.Clear();
    }
    public void Render()
    {
        foreach (var system in _systems) {
            system.Render();
        }
    }
    public void Start()
    {
        foreach (var system in _systems) {
            system.Start();
        }
    }
    public void PhysicsUpdate(float deltaTime)
    {
        foreach (var system in _systems) {
            system.PhysicsUpdate(deltaTime);
        }
    }
    public void Update()
    {
        foreach (var system in _systems) {
            system.Update();
        }
    }
    public void LateUpdate()
    {
        foreach (var system in _systems) {
            system.LateUpdate();
        }
    }
    public void Dispose()
    {
        foreach (var system in _systems) {
            system.Dispose();
        }
    }
}