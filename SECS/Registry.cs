using JetBrains.Annotations;
using SECS.Collections;

namespace SECS;
[PublicAPI]
public sealed class Registry
{
    static Registry()
    {
        Instance = new Registry();
    }
    public static Registry Instance { get; }

    public readonly DirectedAcyclicGraph<Archetype, int> ArchetypeDag;
    public readonly SortedList<EntityId, Archetype>      EntityArchetype;

    public Registry()
    {
        EntityArchetype = new SortedList<EntityId, Archetype>();
        ArchetypeDag = new DirectedAcyclicGraph<Archetype, int>(Archetype.Empty);
    }
    public EntityId CreateEntity()
    {
        var entity = EntityIdGenerator.GetNextId();
        EntityArchetype.Add(entity, Archetype.Empty);
        return entity;
    }
    public void DestroyEntity(EntityId entityId)
    {
        var archetype = EntityArchetype[entityId];
        archetype.RemoveComponentsOfEntity(entityId);
        EntityArchetype.Remove(entityId);
    }
    public void AddComponent<T>(EntityId entityId, T component)
    {
        var oldArchetype = EntityArchetype[entityId];

        var newArchetype = !ArchetypeDag.Root.TryMoveNext(typeof(T).MetadataToken, out var node)
            ? new Archetype(oldArchetype.ArchetypeId + 1)
            : node.Value;

        newArchetype.AddComponent(entityId, component!);
        oldArchetype.CopyComponentsOfEntityToArchetype(entityId, newArchetype);
        oldArchetype.RemoveComponentsOfEntity(entityId);
        ArchetypeDag.Add(oldArchetype, typeof(T).MetadataToken, newArchetype);
        EntityArchetype[entityId] = newArchetype;
    }
}