using System;
using System.Threading;
using Content.Shared;
using Content.Shared.Paddle;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server
{
    [UsedImplicitly]
    public class PongSystem : SharedPongSystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IConfigurationManager _cfgManager = default!;
        
        private float _restartTimer;
        private int _winThreshold;
        private const float BallScoreOffset = 0.9f;
        private const float BallInitialSpeed = 4f;
        private const float PaddleOffset = 1f;
        private MapId _map = MapId.Nullspace;

        private float _endTimer = 0f;

        private MapCoordinates MapCenter => new(ArenaSize / 2f, _map);
        private MapCoordinates PaddleOneStarting => new(new Vector2(PaddleOffset, MapCenter.Y), _map);
        private MapCoordinates PaddleTwoStarting => new(new Vector2(ArenaBox.Size.X - PaddleOffset, MapCenter.Y), _map);
        
        private IPlayerSession? _playerOne;
        private IPlayerSession? _playerTwo;

        private IEntity? _paddleOne;
        private IEntity? _paddleTwo;
        private IEntity? _ball;

        /// <summary>
        ///     Returns the number of players who are connected or in-game.
        /// </summary>
        private int ConnectedPlayers => _playerManager.GetPlayersBy(player => player.Status is SessionStatus.Connected or SessionStatus.InGame).Count;
        
        public float BallScoreSpeedMultiplier
        {
            get
            {
                var score = 0;
                
                foreach (var paddle in ComponentManager.EntityQuery<PaddleComponent>())
                {
                    score += paddle.Score;
                }

                score /= 2;

                return 1f + IoCManager.Resolve<IConfigurationManager>().GetCVar(ContentCVars.BallSpeedupScore) * score;
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();
            _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
            
            _cfgManager.OnValueChanged(ContentCVars.PongRestartTimer, OnPongRestartTimerChanged, true);
            _cfgManager.OnValueChanged(ContentCVars.PongWinThreshold, OnPongWinThresholdChanged, true);
        }

        public override void Shutdown()
        {
            base.Shutdown();
            _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
        }

        private bool IsPlayer(IPlayerSession session)
        {
            return session == _playerOne || session == _playerTwo;
        }

        private void StartGame()
        {
            DebugTools.Assert(State == PongGameState.Start);

            if (ConnectedPlayers < 2)
                return;
            
            Logger.Info("Starting pong game...");
            
            ChangeState(PongGameState.Game);

            var players = _playerManager.GetAllPlayers();

            _playerOne = _random.PickAndTake(players);
            _playerTwo = _random.PickAndTake(players);

            _map = _mapManager.CreateMap();
            
            _ball = MakeBall();
            _ball.GetComponent<PhysicsComponent>().LinearVelocity = Vector2.One * BallInitialSpeed * BallScoreSpeedMultiplier; 

            _paddleOne = MakePaddle(_playerOne, true);
            _paddleTwo = MakePaddle(_playerTwo, false);
            
            foreach (var player in players)
            {
                if(player.Status == SessionStatus.InGame)
                    MakeObserver(player);
            }
        }
        
        private void EndGame()
        {
            DebugTools.Assert(State == PongGameState.Game);

            Logger.Info("Ending pong game...");

            _ball?.Delete();
            _endTimer = _restartTimer;
            
            ChangeState(PongGameState.End);
        }

        private void RestartGame()
        {
            DebugTools.Assert(State == PongGameState.End);

            Logger.Info("Restarting pong game...");
            
            // Clear all entities.
            foreach (var entity in EntityManager.GetEntities())
            {
                entity.Delete();
            }
            
            _map = MapId.Nullspace;

            ChangeState(PongGameState.Start);
        }
        
        private void ChangeState(PongGameState state)
        {
            var old = State;
            State = state;
            
            var ev = new PongGameStateChangedEvent() 
                {Old = old, New = State};
            
            RaiseLocalEvent(ev);
            RaiseNetworkEvent(ev);
            
            Logger.Info($"Pong Game State changed from {old} to {State}!");
        }

        private IEntity MakeBall()
        {
            DebugTools.Assert(State == PongGameState.Game);

            return EntityManager.SpawnEntity("Ball", MapCenter);
        }
        
        private IEntity MakePaddle(IPlayerSession session, bool first)
        {
            DebugTools.Assert(State == PongGameState.Game);
            
            var entity = EntityManager.SpawnEntity("Paddle", first ? PaddleOneStarting : PaddleTwoStarting);
            var paddle = entity.GetComponent<PaddleComponent>();

            paddle.Player = session.Name;
            paddle.First = first;
            paddle.Dirty();
            
            session.AttachToEntity(entity);

            return entity;
        }
        
        private void MakeObserver(IPlayerSession session)
        {
            DebugTools.Assert(State == PongGameState.Game);

            // Empty entity.
            var entity = EntityManager.SpawnEntity(null, MapCenter);
            
            session.AttachToEntity(entity);
        }

        private void EnsurePaddlesHavePlayers()
        {
            DebugTools.Assert(State == PongGameState.Game);

            // Can't do much if player count is below the required.
            if (ConnectedPlayers < 2)
                return;
            
            IPlayerSession PaddleReassign(IEntity paddle, IPlayerSession? ignore)
            {
                var players = _playerManager.GetPlayersBy(s => s != ignore && s.Status == SessionStatus.InGame);

                // No more players to choose from.
                if (players.Count == 0)
                    return null!;
                
                var player = _random.PickAndTake(players);

                player.AttachToEntity(paddle);
                
                var paddleComp = paddle.GetComponent<PaddleComponent>();
                paddleComp.Player = player.Name;
                paddleComp.Dirty();
                
                return player;
            }

            // Delay reattaching the paddles to the next tick.
            Timer.Spawn(0, () =>
            {
                if (_playerOne is not {Status: SessionStatus.InGame})
                {
                    if(_paddleOne!.HasComponent<IActorComponent>())
                        _playerOne?.DetachFromEntity();
                    _playerOne = PaddleReassign(_paddleOne!, _playerTwo);
                }

                if (_playerTwo is not {Status: SessionStatus.InGame})
                {
                    if(_paddleTwo!.HasComponent<IActorComponent>())
                        _playerTwo?.DetachFromEntity();
                    _playerTwo = PaddleReassign(_paddleTwo!, _playerOne);
                }
            }, CancellationToken.None);
        }
        
        private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
        {
            if (args.NewStatus == SessionStatus.Connected)
            {
                args.Session.JoinGame();
                RaiseNetworkEvent(new PongGameStateChangedEvent() {Old = PongGameState.Start, New = State}, args.Session.ConnectedClient);
                
                if(State == PongGameState.Game)
                    MakeObserver(args.Session);
            }
        }
        
        private void OnPongWinThresholdChanged(int threshold)
        {
            _winThreshold = threshold;
        }

        private void OnPongRestartTimerChanged(float time)
        {
            _restartTimer = time;
        }

        public override void Update(float delta)
        {
            switch (State)
            {
                case PongGameState.Start:
                    StartGame();
                    break;
                case PongGameState.Game:
                    // Check that we still have a minimum of two players connected...
                    if (ConnectedPlayers < 2)
                    {
                        EndGame();
                        break;
                    }
                    
                    var ballX = _ball!.Transform.WorldPosition.X;

                    void OnScore()
                    {
                        SoundSystem.Play(Filter.Broadcast(), "/Audio/score.wav", AudioParams.Default);
                        
                        // Reset ball.
                        _ball!.Transform.Coordinates = EntityCoordinates.FromMap(EntityManager, _mapManager, MapCenter);
                        var ballPhysics = _ball.GetComponent<PhysicsComponent>();
                        var speed = ballPhysics.LinearVelocity.Normalized * BallInitialSpeed * BallScoreSpeedMultiplier;
                        ballPhysics.LinearVelocity = Vector2.Zero;
                        _ball.SpawnTimer(1000, () => ballPhysics.LinearVelocity = speed, CancellationToken.None);
                    }
                    
                    var paddleOne = _paddleOne!.GetComponent<PaddleComponent>();
                    var paddleTwo = _paddleTwo!.GetComponent<PaddleComponent>();
                    
                    if (ballX < _paddleOne!.Transform.WorldPosition.X - BallScoreOffset)
                    {
                        paddleTwo.Score++;
                        paddleTwo.Dirty();
                        OnScore();
                    }
                    
                    if (ballX > _paddleTwo!.Transform.WorldPosition.X + BallScoreOffset)
                    {
                        paddleOne.Score++;
                        paddleOne.Dirty();
                        OnScore();
                    }

                    if (paddleOne.Score >= _winThreshold || paddleTwo.Score >= _winThreshold)
                    {
                        EndGame();
                        break;
                    }
                    
                    EnsurePaddlesHavePlayers();
                    
                    break;
                case PongGameState.End:
                    _endTimer -= delta;
                    
                    if(_endTimer <= 0f)
                        RestartGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}