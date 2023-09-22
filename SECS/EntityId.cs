namespace SECS;
public readonly record struct EntityId(int Id)
{
    public static implicit operator int(EntityId id) => id.Id;
}