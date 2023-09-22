using System.Collections;
using System.Runtime.CompilerServices;
namespace SECS.Query;
public sealed class CachedQuery<T> : IEnumerable<T>
{
    internal readonly Registry Registry;

    readonly List<T>        _cachedResults;
    readonly IEnumerator<T> _enumerator;

    int _registryVersion;
    
    CachedQuery(Registry registry, IEnumerator<T> enumerator)
    {
        Registry = registry;
        _enumerator = enumerator;
        _cachedResults = new List<T>();
        _registryVersion = registry.Version;
    }
    
    public CachedQuery(Registry registry, IEnumerable<T> enumerable)
        : this(registry, enumerable.GetEnumerator()) {}

    public IEnumerator<T> GetEnumerator()
    {
        if (!IsCacheValid()) {
            InvalidateCache();
        }

        int index = 0;

        while (true) {
            if (index < _cachedResults.Count) {
                yield return _cachedResults[index];
                index += 1;
            }
            else if (!_enumerator.MoveNext()) {
                yield break;
            }
            else {
                _cachedResults.Add(_enumerator.Current);
            }
        }
    }
    void InvalidateCache()
    {
        _cachedResults.Clear();
        _registryVersion = Registry.Version;
    }
    bool IsCacheValid()
    {
        return _registryVersion == Registry.Version;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}