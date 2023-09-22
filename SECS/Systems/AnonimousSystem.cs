using JetBrains.Annotations;

namespace SECS.Systems;
[PublicAPI]
public sealed class AnonymousSystem : BaseSystem
{
    public Action?        OnStart         { get; set; }
    public Action<float>? OnPhysicsUpdate { get; set; }
    public Action?        OnUpdate        { get; set; }
    public Action?        OnLateUpdate    { get; set; }
    public Action?        OnRender        { get; set; }
    public Action?        OnDispose       { get; set; }

    public override void Start()                        => OnStart?.Invoke();
    public override void PhysicsUpdate(float deltaTime) => OnPhysicsUpdate?.Invoke(deltaTime);
    public override void Update()                       => OnUpdate?.Invoke();
    public override void LateUpdate()                   => OnLateUpdate?.Invoke();
    public override void Render()                       => OnRender?.Invoke();
    public override void Dispose()                      => OnDispose?.Invoke();
}