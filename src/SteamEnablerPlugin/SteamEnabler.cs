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

            if (Terraria.Program.LaunchParameters.TryGetValue("-steam", out string _))
            {
                SocialAPI.Shutdown();
                SocialAPI.Initialize(SocialMode.Steam);
            }
        }
    }
}
