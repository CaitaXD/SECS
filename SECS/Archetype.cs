using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using SECS.Collections;

namespace SECS;
public static class EntityIdGenerator
{
    static int _nextId = 0;
    public static EntityId GetNextId() => new(_nextId++);
}
[PublicAPI]
public readonly record struct Archetype(int ArchetypeId) : IComparable<Archetype>
{
    readonly static        IComparer<Type> TypeComparer = Comparer<Type>.Create((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
    public readonly static Archetype       Empty        = new(0);

    readonly SortedList<Type, ComponentPool> _componentPools = new(TypeComparer);
    readonly SortedList<EntityId, int>       _entityIndex    = new();

    public Span<T> GetComponents<T>() => _componentPools[typeof(T)].AsSpan<T>();
    public bool TryGetComponents<T>(out Span<T> components)
    {
        if (_componentPools.TryGetValue(typeof(T), out var componentPool)) {
            components = componentPool.AsSpan<T>();
            return true;
        }
        components = default;
        return false;
    }

    public void AddComponent<T>(EntityId entityId, T component)
    {
        var type = typeof(T);
        if (_componentPools.TryGetValue(type, out var componentPool)) {
            componentPool.Add(component);
        }
        else {
            componentPool = new ComponentPool();
            componentPool.Add(component);
            _componentPools.Add(type, componentPool);
        }
        _entityIndex[entityId] = componentPool.ElementCount - 1;
    }
    public void RemoveComponentsOfEntity(EntityId entityId)
    {
        if (_entityIndex.TryGetValue(entityId, out var index)) {
            foreach (var (type, componentPool) in _componentPools) {
                componentPool.Remove(type, index);
            }
        }
        _entityIndex.Remove(entityId);
    }
    public void CopyComponentsOfEntityToArchetype(EntityId entityId, Archetype archetype)
    {
        if (_entityIndex.TryGetValue(entityId, out var index)) {
            foreach (var (type, componentPool) in _componentPools) {
                if (!archetype._componentPools.TryGetValue(type, out var archetypeComponentPool)) {
                    archetypeComponentPool = new ComponentPool();
                    archetype._componentPools.Add(type, archetypeComponentPool);
                }
                componentPool.CopyTo(type, index, 1, archetypeComponentPool);
            }
        }
    }
    public int CompareTo(Archetype other) => ArchetypeId.CompareTo(other.ArchetypeId);
    public string SerializeToJson()
    {
        List<object> componentData = new();
        foreach (var (type, componentPool) in _componentPools) {
            for (int i = 0; i < componentPool.ElementCount; i++) {
                componentData.Add(new
                {
                    Type = type.Name,
                    Data = componentPool.Get(type, i)
                });
            }
        }
        var archetypeRecord = new
        {
            ArchetypeId,
            Archetype = componentData.Select(x => x.GetType().GetProperty("Type")!.GetValue(x)).ToArray(),
            Components = componentData
        };
        var jsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };
        return JsonSerializer.Serialize(archetypeRecord, jsonOptions);
    }
}