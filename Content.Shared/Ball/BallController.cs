using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Player;

namespace Content.Shared.Ball;

[UsedImplicitly]
public class BallController : VirtualController
{
    [Dependency] private readonly BallSystem _ballSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        UpdatesBefore.Add(typeof(ArenaController));
    }

    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        foreach (var (_, transform, physics) in EntityManager.EntityQuery<BallComponent, TransformComponent, PhysicsComponent>())
        {
            var y = transform.WorldPosition.Y;

            // Reflect velocity on collision with arena.
            if (!(y > 0) || !(y < SharedPongSystem.ArenaBox.Height))
            {
                physics.LinearVelocity *= new Vector2(1, -1);
                _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), AudioParams.Default.WithVolume(-5f));
            }

            var maxSpeed = _ballSystem.BallMaximumSpeed;

            // Ensure ball doesn't go above the maximum speed.
            if (physics.LinearVelocity.Length > maxSpeed)
                physics.LinearVelocity = physics.LinearVelocity.Normalized * maxSpeed;
        }
    }
}