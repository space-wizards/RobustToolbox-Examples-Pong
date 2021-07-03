using Content.Shared;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Client
{
    [UsedImplicitly]
    public class PongSystem : SharedPongSystem
    {
        public override void Initialize()
        {
            base.Initialize();
            
            SubscribeNetworkEvent<PongGameStateChangedEvent>(OnPongGameStateChanged);
            SubscribeLocalEvent<PlayerAttachSysMessage>(OnPlayerAttached);
        }

        private void OnPongGameStateChanged(PongGameStateChangedEvent ev)
        {
            State = ev.New;

            RaiseLocalEvent(ev);
        }
        
        private void OnPlayerAttached(PlayerAttachSysMessage ev)
        {
            if (ev.AttachedEntity == null)
                return;

            // This will set a camera in the middle of the arena.
            var camera = EntityManager.SpawnEntity(null, new MapCoordinates(ArenaSize/2f, ev.AttachedEntity.Transform.MapID));
            var eye = camera.AddComponent<EyeComponent>();
            eye.Current = true;
            eye.Zoom = Vector2.One;
        }
    }
}