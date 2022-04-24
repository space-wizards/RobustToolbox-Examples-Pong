# Robust Pong
An example multiplayer project for RobustToolbox.


## How to build, and run
### Required Tools (Windows, Linux, Mac)
> Tools needed to build this repository. After installing them, you may need to restart your machine for the build to work.
* [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [Git](https://git-scm.com/downloads)

### Build, and run steps
1. Clone the repository
2. Open a command window in the repository folder
3. Run `git submodule update --init --recursive`
4. Run `dotnet build`
5. Run `dotnet run --project Content.Server` to start the server
6. Open another command window in the repository folder for the client, and run `dotnet run --project Content.Client`
7. When the client window appears, enter a name, and connect to your local server with `127.0.0.1`
8. Repeat steps 6, and 7 to connect a second player
