using Content.Client.Overlays;
using Content.Client.UserInterface;
using Content.Client.UserInterface.Hud;
using Content.Client.UserInterface.States;
using JetBrains.Annotations;
using Robust.Client;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.State;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Client
{
    [UsedImplicitly]
    public class EntryPoint : GameClient
    {
        public override void PreInit()
        { 
            IoCManager.Resolve<IClyde>().SetWindowTitle("Robust Pong");
        }

        public override void Init()
        {
            var factory = IoCManager.Resolve<IComponentFactory>();
            var prototypes = IoCManager.Resolve<IPrototypeManager>();

            factory.DoAutoRegistrations();

            foreach (var ignoreName in IgnoredComponents.List)
            {
                factory.RegisterIgnore(ignoreName);
            }

            foreach (var ignoreName in IgnoredPrototypes.List)
            {
                prototypes.RegisterIgnore(ignoreName);
            }

            ClientContentIoC.Register();

            IoCManager.BuildGraph();
            
            IoCManager.Resolve<StyleSheetManager>().Initialize();
            IoCManager.Resolve<HudManager>().Initialize();
        }

        public override void PostInit()
        {
            base.PostInit();
            
            IoCManager.Resolve<InputHookupManager>().Initialize();

            // Pong doesn't need fancy lighting or FOV.
            IoCManager.Resolve<ILightManager>().Enabled = false;
            
            var overlayManager = IoCManager.Resolve<IOverlayManager>();
            
            // Add the needed overlays.
            overlayManager.AddOverlay(new CollisionOverlay());
            overlayManager.AddOverlay(new ArenaOverlay());
        }
    }
}