using System;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.Ball;

[UsedImplicitly]
public sealed class BallSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
        
    private float _ballSpeedupFactor;

    public float BallMaximumSpeed { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BallComponent, StartCollideEvent>(OnBallCollide);
            
        _cfgManager.OnValueChanged(ContentCVars.BallSpeedup, OnBallSpeedupChanged, true);
        _cfgManager.OnValueChanged(ContentCVars.BallMaximumSpeed, OnBallMaxSpeedChanged, true);
    }

    private void OnBallSpeedupChanged(float factor)
    {
        _ballSpeedupFactor = factor;
    }
        
    private void OnBallMaxSpeedChanged(float maxSpeed)
    {
        BallMaximumSpeed = maxSpeed;
    }

    private void OnBallCollide(EntityUid uid, BallComponent component, ref StartCollideEvent args)
    {
        // Reflect the ball if it has collided with anything and speed it up slightly.
        var physics = EntityManager.GetComponent<PhysicsComponent>(uid);
        
        var (_, y) = args.OtherBody.LinearVelocity;
        var ourVelocity = physics.LinearVelocity;

        // Can't be zero, otherwise the maths don't check out.
        // Reflect direction will depend on positions so it can be predicted accurately by the client.
        if (MathHelper.CloseTo(y, 0f))
        {
            y = _transform.GetWorldPosition(uid).Y > _transform.GetWorldPosition(args.OtherEntity).Y 
                ? 1f : -1f;
        }
        
        var velocity = ourVelocity * new Vector2(-1, MathF.Sign(y) * MathF.Sign(ourVelocity.Y)) * _ballSpeedupFactor;
        _physics.SetLinearVelocity(uid, velocity, true, true, null, physics);

        if (_timing.IsFirstTimePredicted)
        {
            _audioSystem.PlayGlobal("/Audio/bleep.wav", Filter.Broadcast(), true, AudioParams.Default.WithVolume(-5f));
        }
    }
}

[RegisterComponent]
public sealed class BallComponent : Component
{
}