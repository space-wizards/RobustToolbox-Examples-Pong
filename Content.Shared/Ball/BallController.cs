using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Ball;

[UsedImplicitly]
public sealed class BallController : VirtualController
{
    [Dependency] private readonly IGameTiming _timing = default!;
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

        var enumerator = EntityManager.EntityQueryEnumerator<BallComponent, TransformComponent, PhysicsComponent>();
        while (enumerator.MoveNext(out var uid, out var ball, out var transform, out var physics))
        {
            var y = TransformSystem.GetWorldPosition(transform).Y;

            // Reflect velocity on collision with arena.
            if (!(y > 0) || !(y < SharedPongSystem.ArenaBox.Height))
            {
                PhysicsSystem.SetLinearVelocity(uid, physics.LinearVelocity * new Vector2(1, -1), body:physics);
                if (_timing.IsFirstTimePredicted)
                {
                    _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), true, AudioParams.Default.WithVolume(-5f));
                }
            }

            var maxSpeed = _ballSystem.BallMaximumSpeed;

            // Ensure ball doesn't go above the maximum speed.
            if (physics.LinearVelocity.Length > maxSpeed)
                PhysicsSystem.SetLinearVelocity(uid, physics.LinearVelocity.Normalized * maxSpeed, body:physics);
        }
    }
}