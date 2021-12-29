using System;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Player;

namespace Content.Shared.Ball;

[UsedImplicitly]
public class BallSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
        
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

    private void OnBallCollide(EntityUid uid, BallComponent component, StartCollideEvent args)
    {
        // Reflect the ball if it has collided with anything and speed it up slightly.
        var physics = EntityManager.GetComponent<PhysicsComponent>(uid);

        var (_, y) = args.OtherFixture.Body.LinearVelocity;
        var ourVelocity = physics.LinearVelocity;

        // Can't be zero, otherwise the maths don't check out.
        // Reflect direction will depend on positions so it can be predicted accurately by the client.
        if (MathHelper.CloseTo(y, 0f))
        {
            y = Transform(component.Owner).WorldPosition.Y > Transform(args.OtherFixture.Body.Owner).WorldPosition.Y 
                ? 1f : -1f;
        }
            
        physics.LinearVelocity *= new Vector2(-1, MathF.Sign(y) * MathF.Sign(ourVelocity.Y)) * _ballSpeedupFactor;

        SoundSystem.Play(Filter.Broadcast(), "/Audio/bleep.wav", AudioParams.Default.WithVolume(-5f));
    }
}

[RegisterComponent]
public class BallComponent : Component
{
    public override string Name => "Ball";
}