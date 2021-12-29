using Content.Shared;
using Robust.Shared.IoC;

namespace Content.Server;

internal static class ServerContentIoC
{
    public static void Register()
    {
        SharedContentIoC.Register();
    }
}