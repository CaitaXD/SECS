using SECS.Collections;

namespace SECS.Systems;
public abstract class BaseSystem : ISystem, IDisposable
{
    public virtual void Render()                       {}
    public virtual void Start()                        {}
    public virtual void PhysicsUpdate(float deltaTime) {}
    public virtual void Update()                       {}
    public virtual void LateUpdate()                   {}
    public virtual void Dispose()                      {}
}