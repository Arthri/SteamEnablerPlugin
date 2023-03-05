using System;
using System.Reflection;
using Terraria;
using Terraria.Social;
using TerrariaApi.Server;

namespace SteamEnablerPlugin
{
    [ApiVersion(2, 1)]
    public class SteamEnabler : TerrariaPlugin
    {
        public override string Name => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public override string Description => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;

        public SteamEnabler(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            if (Program.LaunchParameters.TryGetValue("-steam", out string _))
            {
                SocialAPI.Shutdown();
                SocialAPI.Initialize(SocialMode.Steam);
            }
        }
    }
}
