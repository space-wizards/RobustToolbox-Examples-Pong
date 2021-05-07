using System;
using Content.Client.UserInterface.Hud;
using Content.Shared;
using Content.Shared.Paddle;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.States
{
    public class GameState : State
    {
        [Dependency] private readonly IComponentManager _componentManager = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
        
        private GameHud? _gameHud;
        private LobbyHud? _lobbyHud;
        
        public override void Startup()
        {
            _gameHud = new GameHud() { Visible = false };
            _lobbyHud = new LobbyHud() {Visible = false };
            
            LayoutContainer.SetAnchorAndMarginPreset(_gameHud, LayoutContainer.LayoutPreset.Wide);
            LayoutContainer.SetAnchorAndMarginPreset(_lobbyHud, LayoutContainer.LayoutPreset.Wide);
            
            _userInterface.StateRoot.AddChild(_gameHud);
            _userInterface.StateRoot.AddChild(_lobbyHud);
        }

        public override void Shutdown()
        {
            _gameHud?.Dispose();
            _lobbyHud?.Dispose();
        }

        public override void FrameUpdate(FrameEventArgs e)
        {
            base.FrameUpdate(e);

            if (_gameHud == null || _lobbyHud == null)
                return;

            var state = EntitySystem.Get<PongSystem>().State;

            switch (state)
            {
                case PongGameState.Game:
                case PongGameState.End:
                    _gameHud.Visible = true;
                    _lobbyHud.Visible = false;
                    break;
                case PongGameState.Start:
                    _gameHud.Visible = false;
                    _lobbyHud.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var gameEnded = state == PongGameState.End;

            if (!gameEnded)
                _gameHud.WinnerLabelText = string.Empty;

            var winningScore = 0;

            foreach (var paddle in _componentManager.EntityQuery<PaddleComponent>())
            {
                // There's only supposed to be two paddle entities.
                if (paddle.First)
                {
                    _gameHud.PlayerOneName = paddle.Player;
                    _gameHud.PlayerOneScore = paddle.Score;
                }
                else
                {
                    _gameHud.PlayerTwoName = paddle.Player;
                    _gameHud.PlayerTwoScore = paddle.Score;
                }
                
                if (!gameEnded)
                    continue;

                if (paddle.Score == winningScore)
                {
                    _gameHud.WinnerLabelText = "It's a draw!";
                    continue;
                }

                if (paddle.Score >= winningScore)
                {
                    winningScore = paddle.Score;
                    _gameHud.WinnerLabelText = $"{paddle.Player} wins!";
                }
            }
        }
    }
}