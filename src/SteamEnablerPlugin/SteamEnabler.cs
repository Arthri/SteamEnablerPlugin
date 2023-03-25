using System.IO.Compression;
using System.Reflection;
using Terraria;
using Terraria.Net.Sockets;
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
            On.Terraria.Net.Sockets.SocialSocket.Terraria_Net_Sockets_ISocket_AsyncReceive += Terraria_Net_Sockets_SocialSocket__Terraria_Net_Sockets_ISocket_AsyncReceive;
            On.Terraria.Net.Sockets.SocialSocket.Terraria_Net_Sockets_ISocket_AsyncSend += Terraria_Net_Sockets_SocialSocket__Terraria_Net_Sockets_ISocket_AsyncSend;

            Console.WriteLine($"Initializing {Name}");

            if (OperatingSystem.IsWindows())
            {
                if (!File.Exists("steam_api64.dll"))
                {
                    Download_SteamworksNET(out string? path);
                    File.Copy(
                        Path.Combine(path, "Windows-x64", "steam_api64.dll"),
                        "steam_api64.dll"
                    );
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                if (!Directory.Exists("steam_api.bundle"))
                {
                    Download_SteamworksNET(out string? path);
                    var bundlePath = Path.Combine(path, "OSX-Linux-x64", "steam_api.bundle");
                    foreach (string directory in Directory.GetDirectories(bundlePath, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(directory.Replace(path, Environment.CurrentDirectory));
                    }

                    foreach (string file in Directory.GetFiles(bundlePath, "*", SearchOption.AllDirectories))
                    {
                        File.Copy(file, file.Replace(path, Environment.CurrentDirectory), true);
                    }
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (!File.Exists("libsteam_api.so"))
                {
                    Download_SteamworksNET(out string? path);
                    File.Copy(
                        Path.Combine(path, "OSX-Linux-x64", "libsteam_api.so"),
                        "libsteam_api.so"
                    );
                }
            }
            else
            {
                Console.WriteLine("UNSUPPORTED OPERATING SYSTEM");
            }

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

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => SocialAPI.Shutdown();
        }

        private void Terraria_Net_Sockets_SocialSocket__Terraria_Net_Sockets_ISocket_AsyncReceive(
            On.Terraria.Net.Sockets.SocialSocket.orig_Terraria_Net_Sockets_ISocket_AsyncReceive orig,
            SocialSocket self,
            byte[] data,
            int offset,
            int size,
            SocketReceiveCallback callback,
            object state
        )
        {
            // UPDATE TODO: Has the vanilla code changed?
            Task.Run(() => self.ReadCallback(data, offset, size, callback, state));
        }

        private void Terraria_Net_Sockets_SocialSocket__Terraria_Net_Sockets_ISocket_AsyncSend(
            On.Terraria.Net.Sockets.SocialSocket.orig_Terraria_Net_Sockets_ISocket_AsyncSend orig,
            SocialSocket self,
            byte[] data,
            int offset,
            int size,
            SocketSendCallback callback,
            object state
        )
        {
            // UPDATE TODO: Has the vanilla code changed?
            SocialAPI.Network.Send(self._remoteAddress, data, size);
            Task.Run(() => callback(state));
        }

        /// <summary>
        /// Downloads Steamworks.NET to the temporary folder and returns its path.
        /// </summary>
        /// <param name="path">The path to the unzipped Steamworks.NET files.</param>
        private static void Download_SteamworksNET(out string path)
        {
            var terrariaAssembly = typeof(Projectile).Assembly;
            var terrariaDependencies = terrariaAssembly.GetReferencedAssemblies();
            var dependency_SteamworksNET = terrariaDependencies.FirstOrDefault(static d => d.Name == "Steamworks.NET");
            if (dependency_SteamworksNET == null)
            {
                Console.WriteLine("Unsupported Terraria version (Terraria doesn't depend on Steamworks.NET.dll).");
                Environment.Exit(1);
                path = null;
                return;
            }
            else
            {
                var version_SteamworksNET = dependency_SteamworksNET.Version;
                if (version_SteamworksNET == null)
                {
                    Console.WriteLine("Steamworks.NET version is undefined. Assuming 20.1.0.0.");
                    version_SteamworksNET = new Version(20, 1, 0, 0);
                }
                if (version_SteamworksNET != new Version(20, 1, 0, 0))
                {
                    Console.WriteLine($"Unsupported Steamworks.NET version \"{version_SteamworksNET}\". Experimentally downloading version nonetheless.");
                }

                var downloadFolder = Path.Combine(Path.GetTempPath(), "Steamworks.NET");
                Directory.CreateDirectory(downloadFolder);
                var unzippedSteamworksNETPath = Path.Combine(downloadFolder, $"{version_SteamworksNET}");
                if (Directory.Exists(unzippedSteamworksNETPath))
                {
                    path = unzippedSteamworksNETPath;
                    return;
                }

                var zippedSteamworksNETPath = $"{unzippedSteamworksNETPath}.zip";
                if (File.Exists(zippedSteamworksNETPath))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(zippedSteamworksNETPath, unzippedSteamworksNETPath);
                        path = unzippedSteamworksNETPath;
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to extract preexisting zip \"{zippedSteamworksNETPath}\". Redownloading zip file.");
                        Console.WriteLine(e.ToString());
                        try
                        {
                            File.Delete(zippedSteamworksNETPath);
                        }
                        catch
                        {
                            Console.WriteLine("Unable to delete corrupted zip file.");
                            Environment.Exit(1);
                            path = null;
                            return;
                        }
                    }
                }

                var downloadVersion = version_SteamworksNET.Revision == 0
                    ? version_SteamworksNET.ToString(3)
                    : version_SteamworksNET.ToString()
                    ;

                try
                {
                    using var fs = new FileStream(
                        zippedSteamworksNETPath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None
                    );
                    using var ns = new HttpClient()
                        .GetAsync($"https://github.com/rlabrecque/Steamworks.NET/releases/download/{downloadVersion}/Steamworks.NET-Standalone_{downloadVersion}.zip")
                        .ConfigureAwait(false).GetAwaiter().GetResult()
                        .EnsureSuccessStatusCode()
                        .Content
                        .ReadAsStream()
                        ;
                    ns.CopyToAsync(fs, new CancellationTokenSource(10000).Token);
                    new ZipArchive(fs, ZipArchiveMode.Read, true).ExtractToDirectory(unzippedSteamworksNETPath);

                    path = unzippedSteamworksNETPath;
                }
                catch
                {
                    Console.WriteLine("Unable to download zip.");
                    try
                    {
                        File.Delete(zippedSteamworksNETPath);
                    }
                    catch
                    {
                    }
                    try
                    {
                        Directory.Delete(unzippedSteamworksNETPath);
                    }
                    catch
                    {
                    }
                    throw;
                }
            }
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
