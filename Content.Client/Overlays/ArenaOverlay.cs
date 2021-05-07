using Content.Shared;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Overlays
{
    [UsedImplicitly]
    public class ArenaOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

        private readonly ShaderInstance _shader;

        public ArenaOverlay()
        {
            IoCManager.InjectDependencies(this);
            _shader = _prototypeManager.Index<ShaderPrototype>("unshaded").Instance();
        }
        
        protected override void Draw(in OverlayDrawArgs args)
        {
            var handle = args.WorldHandle;
            
            handle.UseShader(_shader);
            handle.DrawRect(SharedPongSystem.ArenaBox, Color.White, false);
            handle.UseShader(null);
        }
    }
}