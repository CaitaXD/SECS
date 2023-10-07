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

    public readonly DirectedAcyclicGraph<Archetype, Type> ArchetypeDag;

    public Registry()
    {
        ArchetypeDag = new DirectedAcyclicGraph<Archetype, Type>(Archetype.Empty);
    }
    Archetype FindArchetype(EntityId entityId)
    {
        var root = ArchetypeDag.Root;
        if (root.Value.EntityIds.Contains(entityId)) {
            return root.Value;
        }
        foreach (var node in root.DepthFirstEnumerator) {
            if (node.Value.EntityIds.Contains(entityId)) {
                return node.Value;
            }
        }
        return Archetype.Empty;
    }
    public EntityId CreateEntity()
    {
        var entity = EntityIdGenerator.GetNextId();
        return entity;
    }
    public void DestroyEntity(EntityId entityId)
    {
        var archetype = FindArchetype(entityId);
        archetype.RemoveComponentsOfEntity(entityId);
    }
    public void AddComponent<T>(EntityId entityId, T component)
    {
        var oldArchetype = FindArchetype(entityId);

        var newArchetype = !ArchetypeDag.Root.TryMoveNext(typeof(T), out var node)
            ? new Archetype(oldArchetype.ArchetypeId + 1)
            : node.Value;

        newArchetype.AddComponent(entityId, component!);
        oldArchetype.CopyComponentsOfEntityToArchetype(entityId, newArchetype);
        oldArchetype.RemoveComponentsOfEntity(entityId);
        ArchetypeDag.Add(oldArchetype, typeof(T), newArchetype);
    }
}