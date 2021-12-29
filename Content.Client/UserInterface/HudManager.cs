using Content.Client.UserInterface.States;
using JetBrains.Annotations;
using Robust.Client;
using Robust.Client.State;
using Robust.Shared.IoC;

namespace Content.Client.UserInterface;

/// <summary>
///     Handles changing the UI state depending on whether we're connecting/connected to a server, etc.
/// </summary>
[UsedImplicitly]
public class HudManager
{
    [Dependency] private readonly IGameController _gameController = default!;
    [Dependency] private readonly IBaseClient _client = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;

    public void Initialize()
    {
        _client.RunLevelChanged += ((_, args) =>
        {
            switch (args.NewLevel)
            {
                case ClientRunLevel.InGame:
                case ClientRunLevel.Connected:
                    _stateManager.RequestStateChange<GameState>();
                    break;
                    
                case ClientRunLevel.Initialize when args.OldLevel < ClientRunLevel.Connected:
                    _stateManager.RequestStateChange<MainMenuState>();
                    break;
                    
                // When we disconnect from the server.
                case ClientRunLevel.Error:
                case ClientRunLevel.Initialize when args.OldLevel >= ClientRunLevel.Connected:
                    if (_gameController.LaunchState.FromLauncher)
                    {
                        _stateManager.RequestStateChange<ConnectingState>();
                        break;
                    }
                        
                    _stateManager.RequestStateChange<MainMenuState>();
                    break;
                    
                case ClientRunLevel.Connecting:
                    _stateManager.RequestStateChange<ConnectingState>();
                    break;
            }
        });
    }
}