using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;

namespace Content.Shared.Paddle;

[UsedImplicitly]
public sealed class PaddleController : VirtualController
{
    [Dependency] private readonly PaddleSystem _paddle = default!;
    
    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var speed = _paddle.PaddleSpeed;

        var enumerator = EntityManager.EntityQueryEnumerator<PaddleComponent, PhysicsComponent, TransformComponent>();
        while (enumerator.MoveNext(out var uid, out var paddle, out var physics, out var transform))
        {
            var direction = Vector2.Zero;
                
            // Check if up is pressed.
            if((paddle.Pressed & Button.Up) != 0)
                direction += Vector2.UnitY;
                
            // Check if down is pressed.
            if((paddle.Pressed & Button.Down) != 0)
                direction -= Vector2.UnitY;
                
            PhysicsSystem.SetLinearVelocity(uid, direction * speed, body:physics);

            var worldPos = TransformSystem.GetWorldPosition(transform);
            TransformSystem.SetWorldPosition(transform, new Vector2(paddle.PaddleX, worldPos.Y));
        }
    }
}