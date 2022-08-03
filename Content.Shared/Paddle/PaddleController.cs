using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;

namespace Content.Shared.Paddle;

[UsedImplicitly]
public sealed class PaddleController : VirtualController
{
    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var speed = EntitySystem.Get<PaddleSystem>().PaddleSpeed;
            
        foreach (var (paddle, physics) in EntityManager.EntityQuery<PaddleComponent, PhysicsComponent>())
        {
            var direction = Vector2.Zero;
                
            // Check if up is pressed.
            if((paddle.Pressed & Button.Up) != 0)
                direction += Vector2.UnitY;
                
            // Check if down is pressed.
            if((paddle.Pressed & Button.Down) != 0)
                direction -= Vector2.UnitY;
                
            physics.LinearVelocity = direction * speed;
        }
    }
}