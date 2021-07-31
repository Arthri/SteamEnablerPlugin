/*
    SteamEnablerPlugin, a TSAPI plugin that enables Steam through CLI flags
    Copyright (C) 2021  Arthri

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
            Console.WriteLine("    SteamEnablerPlugin  Copyright (C) 2021  Arthri");
            Console.WriteLine("    This program comes with ABSOLUTELY NO WARRANTY; see SteamEnablerPlugin.COPYING for details");
            Console.WriteLine("    This is free software, and you are welcome to redistribute it");
            Console.WriteLine("    under certain conditions; see SteamEnablerPlugin.COPYING for details.");

            if (Program.LaunchParameters.TryGetValue("-steam", out string _))
            {
                SocialAPI.Shutdown();
                SocialAPI.Initialize(SocialMode.Steam);
            }
        }
    }
}
