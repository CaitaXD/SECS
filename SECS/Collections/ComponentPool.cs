using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SECS.Collections;
public sealed class ComponentPool : IDisposable
{
    const int DefaultCapacity = 256;
    const int DefaultFactor   = 2;

    readonly nint _data;
    int           _elementCount;
    int           _byteSize;
    int           _byteCapacity;

    public int ElementCount => _elementCount;
    public int ByteSize => _byteSize;
    public int ByteCapacity => _byteCapacity;
    public ComponentPool(int byteCapacity = DefaultCapacity)
    {
        _data = Marshal.AllocHGlobal(byteCapacity);
        _byteCapacity = byteCapacity;
    }
    public unsafe void Add<T>(T component)
    {
        if (typeof(T) == typeof(string)) {
            int  offset = _elementCount * nint.Size;
            nint ptr    = Marshal.StringToHGlobalAuto(Unsafe.As<T, string>(ref component));
            Marshal.WriteIntPtr(_data + offset, ptr);
            _byteSize += nint.Size;
            _elementCount += 1;
            return;
        }
        if (!typeof(T).IsValueType) {
            int      offset = _elementCount * nint.Size;
            GCHandle handle = GCHandle.Alloc(component, GCHandleType.Pinned);
            Marshal.WriteIntPtr(_data + offset, (nint)handle);
            _byteSize += nint.Size;
            _elementCount += 1;
            return;
        }
        int size = Unsafe.SizeOf<T>();
        EnsureCapacity(size);
        Unsafe.Write((void*)(_data + _byteSize), component);
        _byteSize += size;
        _elementCount += 1;
    }
    public object Get(Type type, int index)
    {
        if (type == typeof(string)) {
            int  offset = index * nint.Size;
            nint ptr    = Marshal.ReadIntPtr(_data + offset);
            return Marshal.PtrToStringAuto(ptr) ?? string.Empty;
        }
        if (!type.IsValueType) {
            int      offset = index * nint.Size;
            GCHandle handle = GCHandle.FromIntPtr(Marshal.ReadIntPtr(_data + offset));
            return handle.Target!;
        }
        else {
            int offset = index * Marshal.SizeOf(type);
            return Marshal.PtrToStructure(_data + offset, type)!;
        }
    }
    public unsafe ref T Get<T>(int index)
    {
        int offset = index * Unsafe.SizeOf<T>();
        return ref Unsafe.AsRef<T>((void*)(_data + offset));
    }
    public void Clear()
    {
        _byteSize = 0;
    }
    public void Dispose()
    {
        Marshal.FreeHGlobal(_data);
    }
    public unsafe bool Remove(Type type, int index)
    {
        if ((uint)index >= _elementCount) {
            return false;
        }
        else {
            int offset = index * (type.IsClass ? nint.Size : Marshal.SizeOf(type));
            int size   = type.IsClass ? nint.Size : Marshal.SizeOf(type);
            Unsafe.CopyBlock((void*)(_data + offset), (void*)(_data + offset + size), (uint)(_byteSize - offset - size));
            _byteSize -= size;
            _elementCount -= 1;
            return true;
        }
    }
    public unsafe void CopyTo(ComponentPool other)
    {
        Unsafe.CopyBlock((void*)other._data, (void*)_data, (uint)_byteSize);
        other._byteSize = _byteSize;
        other._elementCount = _elementCount;
    }
    public unsafe void CopyTo(Type type, int start, ComponentPool other)
    {
        int offset = start * Marshal.SizeOf(type);
        Unsafe.CopyBlock((void*)other._data, (void*)(_data + offset), (uint)_byteSize);
        other._byteSize = _byteSize;
        other._elementCount = _elementCount;
    }
    public unsafe void CopyTo(Type type, int start, int count, ComponentPool other)
    {
        int offset = start * (type.IsClass ? nint.Size : Marshal.SizeOf(type));
        int size   = count * (type.IsClass ? nint.Size : Marshal.SizeOf(type));
        Unsafe.CopyBlock((void*)other._data, (void*)(_data + offset), (uint)size);
        other._byteSize = size;
        other._elementCount = count;
    }
    public unsafe Span<T> AsSpan<T>()
    {
        return new Span<T>((void*)_data, _elementCount);
    }
    void Grow(int factor = DefaultFactor)
    {
        int capacity = Math.Max(DefaultCapacity, _byteSize * factor);
        Marshal.ReAllocHGlobal(_data, capacity);
        _byteCapacity = capacity;
    }
    void EnsureCapacity(int count)
    {
        if (_byteSize + count >= _byteCapacity) {
            Grow();
        }
    }
}