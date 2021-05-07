using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Shared.Paddle
{
    [UsedImplicitly]
    public class PaddleController : VirtualController
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        
        public override void UpdateBeforeSolve(bool prediction, float frameTime)
        {
            base.UpdateBeforeSolve(prediction, frameTime);

            var speed = EntitySystem.Get<PaddleSystem>().PaddleSpeed;
            
            foreach (var (paddle, physics) in ComponentManager.EntityQuery<PaddleComponent, PhysicsComponent>())
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
}