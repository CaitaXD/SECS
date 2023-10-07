namespace SECS;
public readonly record struct EntityId(int Value) : IComparable<EntityId>
{
    public int CompareTo(EntityId other) => Value.CompareTo(other.Value);
    public static implicit operator int(EntityId id) => id.Value;
}
public readonly record struct Entity;