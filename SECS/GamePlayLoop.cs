using JetBrains.Annotations;
using SECS.Systems;

namespace SECS;
[PublicAPI]
public sealed class GamePlayLoop
{
    const int DefaultCapacity = 4;
    const int DefaultFactor   = 2;

    SystemUpdate[] _systemUpdates;
    SystemStart[]  _systemStarts;

    int _systemUpdateCount;
    int _systemStartCount;
    
    public void AddSystemUpdate<T>(T system) where T : SystemUpdate
    {
        EnsureSystemUpdates(_systemUpdateCount + 1);
        _systemUpdates[_systemUpdateCount++] = system;
    }
    public void AddSystemStart<T>(T system) where T : SystemStart
    {
        EnsureSystemStarts(_systemStartCount + 1);
        _systemStarts[_systemStartCount++] = system;
    }
    public GamePlayLoop()
    {
        _systemUpdates = new SystemUpdate[DefaultCapacity];
        _systemStarts = new SystemStart[DefaultCapacity];
    }
    public void Start()
    {
        foreach (var startOnlySystem in _systemStarts) {
            startOnlySystem.Start();
        }
    }
    public void Update()
    {
        foreach (var physicsUpdateOnlySystem in _systemUpdates) {
            physicsUpdateOnlySystem.Update();
        }
    }
    
    
    void GrowSystemUpdates(int factor = DefaultFactor)
    {
        int capacity = Math.Max(DefaultCapacity, _systemUpdateCount * factor);
        Array.Resize(ref _systemUpdates, capacity);
    }
    void GrowSystemStarts(int factor = DefaultFactor)
    {
        int capacity = Math.Max(DefaultCapacity, _systemStartCount * factor);
        Array.Resize(ref _systemStarts, capacity);
    }
    void EnsureSystemUpdates(int count)
    {
        if (_systemUpdates.Length < count) {
            GrowSystemUpdates();
        }
    }
    void EnsureSystemStarts(int count)
    {
        if (_systemStarts.Length < count) {
            GrowSystemStarts();
        }
    }
}