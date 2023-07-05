using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Overlays;

/// <summary>
///     This is an overlay that draws every entity's hitbox.
///     Since our only entities will be the ball and the paddles, we don't need to use sprites!
/// </summary>
public sealed class CollisionOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _shader;

    public CollisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index<ShaderPrototype>("unshaded").Instance();
    }
        
    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
            
        handle.UseShader(_shader);
        var lookupSystem = _entitySystemManager.GetEntitySystem<EntityLookupSystem>();
        var enumerator = _entityManager.EntityQueryEnumerator<TransformComponent, PhysicsComponent>();
        while (enumerator.MoveNext(out var uid, out var transform, out _))
        {
            var aabb = lookupSystem.GetWorldAABB(uid, transform);
            handle.DrawRect(aabb, Color.White);
        }
            
        handle.UseShader(null);
    }
}