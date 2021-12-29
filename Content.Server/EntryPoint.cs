using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

// DEVNOTE: Games that want to be on the hub are FORCED use the "Content." prefix for assemblies they want to load.
namespace Content.Server;

[UsedImplicitly]
public class EntryPoint : GameServer
{
    public override void Init() {
        base.Init();

        var factory = IoCManager.Resolve<IComponentFactory>();

        factory.DoAutoRegistrations();

        foreach (var ignoreName in IgnoredComponents.List)
        {
            factory.RegisterIgnore(ignoreName);
        }

        ServerContentIoC.Register();

        IoCManager.BuildGraph();
            
        factory.GenerateNetIds();

        // DEVNOTE: This is generally where you'll be setting up the IoCManager further.
    }

    public override void PostInit()
    {
        // Pong doesn't need PVS.
        //IoCManager.Resolve<IConfigurationManager>().SetCVar(CVars.NetPVS, false);
    }

    public override void Update(ModUpdateLevel level, FrameEventArgs frameEventArgs)
    {
        base.Update(level, frameEventArgs);
    }
}