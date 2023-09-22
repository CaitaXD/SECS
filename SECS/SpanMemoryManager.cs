using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SECS;
public unsafe sealed class SpanMemoryManager<T> : MemoryManager<T>
{
    public SpanMemoryManager(Span<T> span)
    {
        _ptr = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        _length = span.Length;
    }

    readonly nint _ptr;
    readonly int  _length;

    public int Length => _length;

    protected override void         Dispose(bool disposing)   {}
    public override    Span<T>      GetSpan()                 => new((void*)_ptr, _length);
    public override    MemoryHandle Pin(int elementIndex = 0) => new((void*)_ptr, default, this);
    public override    void         Unpin()                   {}

    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_length) {
                throw new IndexOutOfRangeException();
            }
            return ref Unsafe.Add(ref Unsafe.AsRef<T>((void*)_ptr), index);
        }
    }
    public static implicit operator Span<T>(SpanMemoryManager<T>   manager) => manager.GetSpan();
    public static implicit operator Memory<T>(SpanMemoryManager<T> manager) => manager.Memory;
}