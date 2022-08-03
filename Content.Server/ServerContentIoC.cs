using Content.Shared;

namespace Content.Server;

internal static class ServerContentIoC
{
    public static void Register()
    {
        SharedContentIoC.Register();
    }
}