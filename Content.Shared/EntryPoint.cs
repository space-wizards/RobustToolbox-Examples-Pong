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
public sealed class EntryPoint : GameShared
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly ILocalizationManager _localizationManager = default!;
    private const string Culture = "en-US";

    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
            
        _localizationManager.LoadCulture(new CultureInfo(Culture));
        
        if (_netManager.IsServer)
        {
            // No need for PVS, this is pong...
            _configManager.SetCVar(CVars.NetPVS, false);
        }
    }
}