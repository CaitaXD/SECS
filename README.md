# SECS
Simple ECS

Usage(Using Raylib Bindings for C#):

Declaring Components
Just plain old structs/classes, avoid using primitive types since you can only have one instance of a given component type on a instance
```
public struct BoxMesh
{
    public int Width;
    public int Height;
}
public struct Transform2D
{
    public Vector2 Position = Vector2.Zero;
    public Vector2 Scale    = Vector2.One;
    public float   Rotation = 0f;
    public Transform2D() {}
}
```

Systems 
Just inherit from BaseSystem or pass callbacks to AnonymousSystem
```
public class InputSystem : BaseSystem
{
    readonly Registry _registry = Registry.Instance;

    readonly IEnumerable<EntityHandle> _transformQuery;
    public InputSystem()
    {
        _transformQuery = _registry.EntityQuery.Where(x => x.HasComponent<Transform2D>());
    }
    
    const float dtFactor = 1_000;
    
    public override void PhysicsUpdate(float deltaTime)
    {
        var transforms = _registry.Components<Transform2D>();
        foreach (var e in _transformQuery) {
            ref var transform = ref transforms[e.Id];
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) {
                transform.Position.Y -= deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) {
                transform.Position.Y += deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) {
                transform.Position.X -= deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) {
                transform.Position.X += deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) {
                transform.Rotation -= deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_E)) {
                transform.Rotation += deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_Z)) {
                transform.Scale.X -= 0.01f * deltaTime * dtFactor;
                transform.Scale.Y -= 0.01f * deltaTime * dtFactor;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_X)) {
                transform.Scale.X += 0.01f * deltaTime * dtFactor;
                transform.Scale.Y += 0.01f * deltaTime * dtFactor;
            }
        }
    }
}
```

```
public sealed class RenderingBaseSystem : BaseSystem
{
    readonly Registry _registry = Registry.Instance;

    readonly IEnumerable<EntityHandle> _boxMeshes;
    public RenderingBaseSystem()
    {
        _boxMeshes = _registry.EntityQuery
            .Where(x => x.HasComponent<BoxMesh>() && x.HasComponent<Transform2D>());
    }
    public override void Render()
    {
        var squares    = _registry.Components<BoxMesh>();
        var transforms = _registry.Components<Transform2D>();

        foreach (var e in _boxMeshes) {
            var square    = squares[e.Id];
            var transform = transforms[e.Id];
            var rect = new Rectangle
            {
                x = transform.Position.X - (square.Width - square.Width / 2),
                y = transform.Position.Y - (square.Height - square.Height / 2),
                width = square.Width * transform.Scale.X,
                height = square.Height * transform.Scale.Y
            };
            
            Raylib.DrawRectanglePro(rect, new Vector2(rect.width / 2, rect.height / 2), transform.Rotation, Color.WHITE);
        }
    }
}
```
Main Loop
```
var game = new Game();

game.AddSystem<RenderingBaseSystem>();
game.AddSystem<InputSystem>();

var player = Registry.Instance.CreateEntity();
var enemy  = Registry.Instance.CreateEntity();

player.AddComponent(new BoxMesh { Width = 100, Height = 100 });
player.AddComponent(new Transform2D { Position = new Vector2(100, 100) });

enemy.AddComponent(new BoxMesh { Width = 100, Height = 100 });
enemy.AddComponent(new Transform2D { Position = new Vector2(25, 50) });

Raylib.InitWindow(800, 600, "Spell");
game.Start();
while (!Raylib.WindowShouldClose()) {
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.LIGHTGRAY);
    game.Render();
    game.PhysicsUpdate(Raylib.GetFrameTime());
    game.Update();
    game.LateUpdate();
    Raylib.EndDrawing();
}
game.Dispose();
Raylib.CloseWindow();
```
