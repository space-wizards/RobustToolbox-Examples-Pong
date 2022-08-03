using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared;

public abstract class SharedPongSystem : EntitySystem
{
    public static readonly Vector2 ArenaSize = new Vector2i(20, 10);
    public static readonly Box2 ArenaBox = Box2.FromDimensions(Vector2.Zero, ArenaSize);
        
    public PongGameState State { get; protected set; } = PongGameState.Start;
}
    
[Serializable, NetSerializable]
public enum PongGameState
{
    Start,
    Game,
    End,
}

[Serializable, NetSerializable]
public sealed class PongGameStateChangedEvent : EntityEventArgs
{
    public PongGameState Old { get; init; }
    public PongGameState New { get; init; }
}