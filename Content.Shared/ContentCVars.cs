using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared
{
    [CVarDefs]
    public sealed class ContentCVars: CVars
    {
        // ----- BALL CVARS -----
        /// <summary>
        ///     Factor by which the ball speed is multiplied every time it collides with a paddle.
        /// </summary>
        public static readonly CVarDef<float> BallSpeedup =
            CVarDef.Create("ball.speedup", 1.15f, CVar.REPLICATED | CVar.SERVER);

        /// <summary>
        ///     The ball will be sped up based on the average score.
        /// </summary>
        public static readonly CVarDef<float> BallSpeedupScore =
            CVarDef.Create("ball.speedup_score", 0.05f, CVar.REPLICATED | CVar.SERVER);

        /// <summary>
        ///     Maximum speed the ball will move at.
        /// </summary>
        public static readonly CVarDef<float> BallMaximumSpeed =
            CVarDef.Create("ball.maximum_speed", 20f, CVar.REPLICATED | CVar.SERVER);
        
        // ----- PONG CVARS -----
        
        /// <summary>
        ///     Number of points a player has to score to win.
        /// </summary>
        public static readonly CVarDef<int> PongWinThreshold =
            CVarDef.Create("pong.win_threshold", 10, CVar.SERVERONLY);
        
        /// <summary>
        ///     Time to wait after the game has ended before restarting.
        /// </summary>
        public static readonly CVarDef<float> PongRestartTimer =
            CVarDef.Create("pong.restart_timer", 10f, CVar.SERVERONLY);
        
        // ----- PADDLE CVARS -----
        
        /// <summary>
        ///     Paddle movement speed.
        /// </summary>
        public static readonly CVarDef<float> PaddleSpeed =
            CVarDef.Create("pong.paddle_speed", 7f, CVar.REPLICATED | CVar.SERVER);
    }
}