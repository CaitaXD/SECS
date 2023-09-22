using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using SECS.Collections;
using SECS.Query;

namespace SECS;
[PublicAPI]
public sealed class Registry
{
    public static Registry Instance { get; } = new();

    public readonly TypeErasedLookup Lookup;
    public Registry()
    {
        Lookup = new TypeErasedLookup();
    }

    public int EntityCount    => _entityList.Count;
    public int ComponentCount => Lookup.Storages.Sum(x => x.Count);

    List<(EntityId e, ComponentHandle c)?> RelationshipList { get; } = new();

    List<EntityId> _entityList = new();

    public int Version { get; private set; }

    public EntityHandle CreateEntity()
    {
        var id = new EntityId(_entityList.Count);
        _entityList.Add(id);
        return new EntityHandle(this, id);
    }
    public ref T AddComponent<T>(EntityId entityId, T component)
    {
        GuardAgainstType<T>();
        Version++;
        var handle          = Lookup.Register(component);
        var componentHandle = new ComponentHandle(handle);

        RelationshipList.Add((entityId, componentHandle));

        return ref Lookup.GetSpan<T>()[handle];
    }
    public ref readonly T GetComponent<T>(EntityId entityId)
    {
        GuardAgainstType<T>();

        var v = RelationshipList.Find(x => x?.e == entityId);
        if (v == null) {
            return ref Unsafe.NullRef<T>();
        }
        var componentHandle = v.Value.c;
        return ref Lookup.GetSpan<T>()[componentHandle.Handle];
    }
    public bool HasComponent<T>(EntityId entityId)
    {
        GuardAgainstType<T>();

        var v = RelationshipList.Find(x => x?.e == entityId);
        if (v == null) {
            return false;
        }
        var componentHandle = v.Value.c;
        return componentHandle.Handle < Lookup.GetSpan<T>().Length;
    }
    public bool TryGetComponent<T>(EntityId entityId, out T component)
    {
        GuardAgainstType<T>();

        var v = RelationshipList.Find(x => x?.e == entityId);
        if (v == null) {
            component = default!;
            return false;
        }
        var componentHandle = v.Value.c;
        if (componentHandle.Handle >= Lookup.GetSpan<T>().Length) {
            component = default!;
            return false;
        }
        component = Lookup.GetSpan<T>()[componentHandle.Handle];
        return true;
    }
    public void RemoveComponent<T>(EntityId entityId)
    {
        GuardAgainstType<T>();
        Version++;
        var v = RelationshipList.Find(x => x?.e == entityId);
        if (v == null) {
            return;
        }
        var componentHandle = v.Value.c;
        var array           = Lookup.GetList<T>();
        array.RemoveAt(componentHandle.Handle);
    }
    public void DestroyEntity(EntityId entityId)
    {
        Version++;
        foreach (var storage in Lookup.Storages) {
            var v = RelationshipList.Find(x => x?.e == entityId);
            if (v == null) {
                continue;
            }
            var componentHandle = v.Value.c;
            storage.RemoveAt(componentHandle.Handle);
        }
    }
    public void ClearRegistry()
    {
        Version++;
        _entityList.Clear();
        Lookup.Clear();
    }
    public IReadOnlyList<EntityId> Entities => _entityList;
    public Span<T> Components<T>()
    {
        GuardAgainstType<T>();

        return Lookup.GetSpan<T>();
    }
    public bool HasComponent(Type type, EntityId entityId)
    {
        GuardAgainstType(type);

        var v = RelationshipList.Find(x => x?.e == entityId);
        if (v == null) {
            return false;
        }
        var componentHandle = v.Value.c;
        return componentHandle.Handle < Lookup.GetList(type).Count;
    }
    public CachedQuery<EntityHandle> EntityQuery => new(this, _entityList.Select(x => new EntityHandle(this, x)));
    static void GuardAgainstType<T>()
    {
        if (typeof(T) == typeof(EntityId) || typeof(T) == typeof(ComponentHandle)) {
            throw new ArgumentException($"What even are you doing? {typeof(T)} is not a valid component type.");
        }
    }
    static void GuardAgainstType(Type type)
    {
        if (type == typeof(EntityId) || type == typeof(ComponentHandle)) {
            throw new ArgumentException($"What even are you doing? {type} is not a valid component type.");
        }
    }
}