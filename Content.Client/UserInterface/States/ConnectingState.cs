#nullable enable
using Content.Client.UserInterface.Hud;
using Robust.Client;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Network;

namespace Content.Client.UserInterface.States;

public class ConnectingState : State
{
    [Dependency] private readonly IGameController _gameController = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IBaseClient _baseClient = default!;

    private ConnectingHud? _connectingHud;

    public override void Startup()
    {
        _connectingHud = new ConnectingHud();
            
        LayoutContainer.SetAnchorAndMarginPreset(_connectingHud, LayoutContainer.LayoutPreset.Wide);
            
        _userInterface.StateRoot.AddChild(_connectingHud);

        _netManager.ClientConnectStateChanged += OnConnectStateChanged;
        _netManager.ConnectFailed += OnConnectFailed;
    }

    private void OnConnectFailed(object? _, NetConnectFailArgs e)
    {
        SetDisconnected();
        _connectingHud?.ShowMessage($"Error:\n{e.Reason}");
    }

    private void OnConnectStateChanged(ClientConnectionState state)
    {
        switch(state)
        {
            case ClientConnectionState.NotConnecting:
                SetDisconnected();
                _connectingHud?.ShowMessage($"Error:\n{_baseClient.LastDisconnectReason ?? "Not connecting."}");
                break;
            case ClientConnectionState.ResolvingHost:
                _connectingHud?.ShowMessage("Connecting...\nResolving host.");
                break;
            case ClientConnectionState.EstablishingConnection:
                _connectingHud?.ShowMessage("Connecting...\nEstablishing connection.");
                break;
            case ClientConnectionState.Handshake:
                _connectingHud?.ShowMessage("Connecting...\nPerforming handshake.");
                break;
            case ClientConnectionState.Connected:
                _connectingHud?.ShowMessage("Connected to the server.");
                break;
        }
    }

    public override void Shutdown()
    {
        _netManager.ConnectFailed -= OnConnectFailed;
        _netManager.ClientConnectStateChanged -= OnConnectStateChanged;
            
        _connectingHud?.Dispose();
    }

    public void SetDisconnected()
    {
        if (_connectingHud == null) return;
        _connectingHud.ReconnectButton.Visible = true;
        _connectingHud.ReconnectButton.OnPressed += _ =>
        {
            if (_gameController.LaunchState.ConnectEndpoint != null)
                _baseClient.ConnectToServer(_gameController.LaunchState.ConnectEndpoint);
                
            _connectingHud.ReconnectButton.Visible = false;
        };
    }
}