using System.Globalization;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

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
        
        // No need for PVS, this is pong...
        IoCManager.Resolve<IConfigurationManager>().SetCVar(CVars.NetPVS, false);
    }
}