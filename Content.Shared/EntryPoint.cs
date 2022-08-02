using System.Globalization;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Network;

// DEVNOTE: Games that want to be on the hub can change their namespace prefix in the "manifest.yml" file.
namespace Content.Shared;

[UsedImplicitly]
public class EntryPoint : GameShared
{
    private const string Culture = "en-US";

    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
            
        IoCManager.Resolve<ILocalizationManager>().LoadCulture(new CultureInfo(Culture));
    }

    public override void Init()
    {
        base.Init();
        var netManager = IoCManager.Resolve<INetManager>();
        var configManager = IoCManager.Resolve<IConfigurationManager>();
        
        if (netManager.IsServer)
        {
            // No need for PVS, this is pong...
            configManager.SetCVar(CVars.NetPVS, false);
        }
    }
}