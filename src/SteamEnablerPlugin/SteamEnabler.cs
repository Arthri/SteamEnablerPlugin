using System.Reflection;
using Terraria;
using Terraria.Social;
using TerrariaApi.Server;

namespace SteamEnablerPlugin
{
    [ApiVersion(2, 1)]
    public class SteamEnabler : TerrariaPlugin
    {
        public override string Name { get; } = typeof(SteamEnabler).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? nameof(SteamEnablerPlugin);

        public override string Description { get; } = typeof(SteamEnabler).Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";

        public override Version Version { get; } = typeof(SteamEnabler).Assembly.GetName()?.Version ?? new Version(1, 0);

        public override string Author { get; } = typeof(SteamEnabler).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Arthri";

        public SteamEnabler(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Console.WriteLine($"Initializing {Name}");

            {
                using var fs = new FileStream("steam_appid.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                if (fs.Length <= 0)
                {
                    using var writer = new StreamWriter(fs);
                    writer.Write("105600");
                }
                else
                {
                    using var reader = new StreamReader(fs);
                    var appId = reader.ReadToEnd();
                    if (appId != "105600")
                    {
                        Console.WriteLine("\"steam_appid.txt\" contains unexpected content");
                    }
                }
            }

            Initialize_Steam();
        }

        // Steam-related code must run in a separate method to prevent JIT
        // from compiling Steamworks.NET and trying to find a possibly
        // missing steam_api64.dll
        private static void Initialize_Steam()
        {
            if (Terraria.Program.LaunchParameters.TryGetValue("-steam", out string _))
            {
                SocialAPI.Shutdown();

                // Detects if Steam is inaccessible.
                // The Terraria client checks for this, but the error is given
                // in the form of a MessageBox which is ignored by OTAPI.
                if (!Steamworks.SteamAPI.Init())
                {
                    Console.WriteLine("Could not initialize Steam.");
                    Console.WriteLine("Possible reasons include but are not limited to");
                    Console.WriteLine("  The Steam client isn't running;");
                    Console.WriteLine("  The Steam client isn't running in the same OS user context;");
                    Console.WriteLine("  The Steam client couldn't find Terraria's app ID. Please add a \"steam_appid.txt\" file with the contents \"105600\" in the server's working directory;");
                    Console.WriteLine("  The \"steam_appid.txt\" file exists but contains the wrong content;");
                    Console.WriteLine("  The Steam client doesn't have access to run Terraria;");
                    Console.WriteLine("  For more information, visit https://partner.steamgames.com/doc/api/steam_api#SteamAPI_Init.");
                    Environment.Exit(0);
                }

                SocialAPI.Initialize(SocialMode.Steam);
            }
        }
    }
}
