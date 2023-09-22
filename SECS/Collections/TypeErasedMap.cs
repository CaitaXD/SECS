using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace SECS.Collections;
[PublicAPI]
public readonly struct TypeErasedMap<TKey, TValue>
{
    public readonly record struct TypeErasedKey(Type Type, TKey Key);
    readonly Dictionary<TypeErasedKey, TValue> _dictionary;

    static TypeErasedKey Key<T>(TKey key) => new(typeof(T), key);
    public TypeErasedMap()
    {
        _dictionary = new Dictionary<TypeErasedKey, TValue>();
    }
    public TValue Get<T>(TKey key)
    {
        return _dictionary[new TypeErasedKey(typeof(T), key)];
    }
    public bool TryGet<T>(TKey key, [NotNullWhen(true)] out TValue value)
    {
        return _dictionary.TryGetValue(Key<T>(key), out value!);
    }
    public void Add<T>(TKey key, TValue value)
    {
        _dictionary.Add(Key<T>(key), value);
    }
    public bool Contains<T>(TKey key)
    {
        return _dictionary.ContainsKey(Key<T>(key));
    }
    public bool Remove<T>(TKey key)
    {
        return _dictionary.Remove(Key<T>(key));
    }
    public bool TryGetValue<T>(TKey key, [NotNullWhen(true)] out TValue value)
    {
        return _dictionary.TryGetValue(Key<T>(key), out value!);
    }
    public void Clear()
    {
        _dictionary.Clear();
    }
    public TValue Get(Type type, TKey key)
    {
        return _dictionary[new TypeErasedKey(type, key)];
    }
}