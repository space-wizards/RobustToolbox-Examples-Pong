using System;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;

namespace Content.Shared;

/// <summary>
///     Ensures no game objects go out of bounds.
/// </summary>
[UsedImplicitly]
public sealed class ArenaController : VirtualController
{
    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        foreach (var (transform, _) in EntityManager.EntityQuery<TransformComponent, PhysicsComponent>())
        {
            if (transform.ParentUid == EntityUid.Invalid)
                return;
                
            var (x, y) = TransformSystem.GetWorldPosition(transform);

            if (SharedPongSystem.ArenaBox.Contains(x, y))
                continue;

            x = MathF.Max(0, MathF.Min(SharedPongSystem.ArenaBox.Width, x));
            y = MathF.Max(0, MathF.Min(SharedPongSystem.ArenaBox.Height, y));
            TransformSystem.SetWorldPosition(transform, new Vector2(x, y));
        }
    }
}