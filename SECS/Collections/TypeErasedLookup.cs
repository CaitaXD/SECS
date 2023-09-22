using System.Collections;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SECS.Collections;
[PublicAPI]
public readonly struct TypeErasedLookup
{
    readonly Dictionary<Type, IList> _storages;
    public   ICollection<IList>      Storages => _storages.Values;
    public   ICollection<Type>       Types    => _storages.Keys;
    public TypeErasedLookup()
    {
        _storages = new Dictionary<Type, IList>();
    }
    public List<T> GetList<T>()
    {
        ref var storage = ref CollectionsMarshal.GetValueRefOrAddDefault(_storages, typeof(T), out bool exists);
        if (!exists) {
            storage = new List<T>();
        }
        return (List<T>)storage!;
    }
    public IList GetList(Type type)
    {
        ref var storage = ref CollectionsMarshal.GetValueRefOrAddDefault(_storages, type, out bool exists);
        if (!exists) {
            storage = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        }
        return storage!;
    }
    public Span<T> GetSpan<T>()
    {
        ref var storage = ref CollectionsMarshal.GetValueRefOrAddDefault(_storages, typeof(T), out bool exists);
        if (!exists) {
            storage = new List<T>();
        }
        return CollectionsMarshal.AsSpan((List<T>)storage!);
    }
    public int Register<T>(T component)
    {
        var list = GetList<T>();
        list.Add(component);
        return list.Count - 1;
    }
    public bool Contains<T>()
    {
        return _storages.ContainsKey(typeof(T));
    }
    public bool Contains(Type type)
    {
        return _storages.ContainsKey(type);
    }
    public void Clear()
    {
        foreach (var storage in _storages.Values) {
            storage.Clear();
        }
    }
}