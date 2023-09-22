namespace SECS.Systems;
public interface ISystem
{
    void Render();
    void Start();
    void PhysicsUpdate(float deltaTime);
    void Update();
    void LateUpdate();
    void Dispose();
}