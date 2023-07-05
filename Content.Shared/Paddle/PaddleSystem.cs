using System;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.IoC;
using Robust.Shared.Players;
using Robust.Shared.Serialization;

namespace Content.Shared.Paddle;

/// <summary>
///     Controls player movement.
///     Since this is in shared, it will be predicted on the client and reconciled if needed.
/// </summary>
[UsedImplicitly]
public sealed class PaddleSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    public float PaddleSpeed { get; private set; }
        
    public override void Initialize()
    {
        base.Initialize();
            
        SubscribeLocalEvent<PaddleComponent, ComponentGetState>(GetPaddleState);
        SubscribeLocalEvent<PaddleComponent, ComponentHandleState>(HandlePaddleState);
            
        _cfgManager.OnValueChanged(ContentCVars.PaddleSpeed, OnPaddleSpeedChanged, true);
            
        CommandBinds.Builder
            .Bind(EngineKeyFunctions.MoveUp, new ButtonInputCmdHandler(Button.Up, SetMovementInput))
            .Bind(EngineKeyFunctions.MoveDown, new ButtonInputCmdHandler(Button.Down, SetMovementInput))
            .Register<PaddleSystem>();
    }

    private void GetPaddleState(EntityUid uid, PaddleComponent component, ref ComponentGetState args)
    {
        args.State = new PaddleComponentState(component.Score, component.Player, component.First, component.Pressed, component.PaddleX);
    }

    private void HandlePaddleState(EntityUid uid, PaddleComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not PaddleComponentState state)
            return;

        component.Score = state.Score;
        component.Player = state.Player;
        component.First = state.First;
        component.PaddleX = state.PaddleX;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<PaddleSystem>();
    }
        
    private void OnPaddleSpeedChanged(float speed)
    {
        PaddleSpeed = speed;
    }

    private void SetMovementInput(ICommonSession? session, Button button, bool state)
    {
        if (session?.AttachedEntity == null 
            || Deleted(session.AttachedEntity) 
            || !TryComp<PaddleComponent>(session.AttachedEntity, out var paddle))
            return;

        if (state)
            paddle.Pressed |= button;
        else
            paddle.Pressed &= ~button;
            
        Dirty(session.AttachedEntity.Value, paddle);
    }

    private sealed class ButtonInputCmdHandler : InputCmdHandler
    {
        public delegate void MoveDirectionHandler(ICommonSession? session, Button button, bool state);
            
        private readonly Button _button;
        private readonly MoveDirectionHandler _handler;
            
        public ButtonInputCmdHandler(Button button, MoveDirectionHandler handler)
        {
            _button = button;
            _handler = handler;
        }
            
        public override bool HandleCmdMessage(ICommonSession? session, InputCmdMessage message)
        {
            if (message is not FullInputCmdMessage full)
                return false;
                
            _handler.Invoke(session, _button, full.State == BoundKeyState.Down);
            return false;
        }
    }
}

[RegisterComponent, NetworkedComponent]
public sealed class PaddleComponent : Component
{
    public Button Pressed { get; set; } = Button.None;
    public int Score { get; set; } = 0;
    public string Player { get; set; } = string.Empty;
    public bool First { get; set; } = false;
    public float PaddleX { get; set; } = 0;
}

[Serializable, NetSerializable]
public sealed class PaddleComponentState : ComponentState
{
    public int Score { get; }
    public string Player { get; }
    public bool First { get; }
    public Button Pressed { get; }
    public float PaddleX { get; set; } = 0;
        
    public PaddleComponentState(int score, string player, bool first, Button pressed, float paddleX)
    {
        Score = score;
        Player = player;
        First = first;
        Pressed = pressed;
        PaddleX = paddleX;
    }
}

[Flags, Serializable, NetSerializable]
public enum Button
{
    None = 0,
    Up = 1,
    Down = 2,
}