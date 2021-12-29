using Content.Client.UserInterface;
using Content.Shared;
using Robust.Shared.IoC;

namespace Content.Client;

internal static class ClientContentIoC
{
    public static void Register()
    {
        SharedContentIoC.Register();
            
        IoCManager.Register<HudManager, HudManager>();
        IoCManager.Register<StyleSheetManager, StyleSheetManager>();
        IoCManager.Register<InputHookupManager, InputHookupManager>();
    }
}