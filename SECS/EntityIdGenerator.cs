namespace SECS;
public struct EntityIdGenerator
{
    int _nextId;
    public EntityId GetNextId() => new(_nextId++);
}