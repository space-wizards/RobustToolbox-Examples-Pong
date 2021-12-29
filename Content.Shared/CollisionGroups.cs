using System;
using JetBrains.Annotations;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization;

namespace Content.Shared;

/// <summary>
///     Defined collision groups for the physics system.
/// </summary>
[Flags, PublicAPI]
[FlagsFor(typeof(CollisionLayer)), FlagsFor(typeof(CollisionMask))]
public enum CollisionGroup
{
        
    None = 0,
    Solid = 1 << 0,
}