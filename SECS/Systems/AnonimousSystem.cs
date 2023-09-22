using JetBrains.Annotations;

namespace SECS.Systems;
[PublicAPI]
public sealed class AnonymousSystem : BaseSystem
{
    public Action?        OnStart         { get; init; }
    public Action<float>? OnPhysicsUpdate { get; init; }
    public Action?        OnUpdate        { get; init; }
    public Action?        OnLateUpdate    { get; init; }
    public Action?        OnRender        { get; init; }
    public Action?        OnDispose       { get; init; }

    public override void Start()                        => OnStart?.Invoke();
    public override void PhysicsUpdate(float deltaTime) => OnPhysicsUpdate?.Invoke(deltaTime);
    public override void Update()                       => OnUpdate?.Invoke();
    public override void LateUpdate()                   => OnLateUpdate?.Invoke();
    public override void Render()                       => OnRender?.Invoke();
    public override void Dispose()                      => OnDispose?.Invoke();
}