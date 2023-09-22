﻿namespace SECS.Query;
public readonly record struct EntityHandle(Registry Registry, EntityId Id)
{
    public ref readonly T    GetComponent<T>()            => ref Registry.GetComponent<T>(Id);
    public              bool HasComponent<T>()            => Registry.HasComponent<T>(Id);
    public ref          T    AddComponent<T>(T component) => ref Registry.AddComponent(Id, component);
}