using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace AotForms
{
    internal static class ConfigPipe
    {
        private const string PipeName = "BlackCorps_Config";
        private static Thread? _serverThread;
        private static bool _running = true;

        public static void StartServer()
        {
            _serverThread = new Thread(ServerLoop)
            {
                IsBackground = true
            };
            _serverThread.Start();
        }

        public static void Stop()
        {
            _running = false;
            _serverThread?.Join(1000);
        }

        private static void ServerLoop()
        {
            while (_running)
            {
                try
                {
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                    server.WaitForConnection();

                    using var reader = new StreamReader(server, Encoding.UTF8);
                    while (_running && server.IsConnected)
                    {
                        string? line = reader.ReadLine();
                        if (line == null) break;

                        ParseConfigLine(line);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                }
            }
        }

        private static void ParseConfigLine(string line)
        {
            try
            {
                int eqIdx = line.IndexOf('=');
                if (eqIdx < 0) return;

                string key = line.Substring(0, eqIdx).Trim();
                string val = line.Substring(eqIdx + 1).Trim();

                switch (key)
                {
                    case "StreamerMode":
                        Config.StreamerMode = bool.Parse(val);
                        break;
                    case "ESPBox":
                        Config.ESPBox = bool.Parse(val);
                        break;
                    case "ESPSkeleton":
                        Config.ESPSkeleton = bool.Parse(val);
                        break;
                    case "ESPName":
                        Config.ESPName = bool.Parse(val);
                        break;
                    case "ESPLine":
                        Config.ESPLine = bool.Parse(val);
                        break;
                    case "ESPHealth":
                        Config.ESPHealth = bool.Parse(val);
                        break;
                    case "ESPDistance":
                        Config.ESPDistance = bool.Parse(val);
                        break;
                    case "ESPHealthText":
                        Config.ESPHealthText = bool.Parse(val);
                        break;
                    case "ESPFillBox":
                        Config.ESPFillBox = bool.Parse(val);
                        break;
                    case "ESPBox2":
                        Config.ESPBox2 = bool.Parse(val);
                        break;
                    case "ESPShuriken":
                        Config.ESPShuriken = bool.Parse(val);
                        break;
                    case "ESPShurikenRotate":
                        Config.ESPShurikenRotate = bool.Parse(val);
                        break;
                    case "ESPInformation":
                        Config.ESPInformation = bool.Parse(val);
                        break;
                    case "espweapon":
                        Config.espweapon = bool.Parse(val);
                        break;
                    case "EnemyCount":
                        Config.EnemyCount = bool.Parse(val);
                        break;
                    case "ESPInfoBox":
                        Config.ESPInfoBox = bool.Parse(val);
                        break;
                    case "MiniMap":
                        Config.Minimap = bool.Parse(val);
                        break;
                    case "CrosshairEnabled":
                        Config.CrosshairEnabled = bool.Parse(val);
                        break;
                    case "FOVEnabled":
                        Config.FOVEnabled = bool.Parse(val);
                        break;
                    case "Aimfovc":
                        Config.Aimfovc = bool.Parse(val);
                        break;
                    case "FixEsp":
                        Config.FixEsp = bool.Parse(val);
                        break;
                    case "EspUp":
                        Config.EspUp = bool.Parse(val);
                        break;
                    case "EspBottom":
                        Config.EspBottom = bool.Parse(val);
                        break;
                    case "EspMiddle":
                        Config.EspMiddle = bool.Parse(val);
                        break;
                    case "UpdateEntities":
                        Config.UpdateEntities = bool.Parse(val);
                        break;
                    case "enableAimBot":
                        Config.enableAimBot = bool.Parse(val);
                        break;
                    case "AimBotRage":
                        Config.AimBotRage = bool.Parse(val);
                        break;
                    case "SilentAim":
                        Config.SilentAim = bool.Parse(val);
                        break;
                    case "AimbotVisible":
                        Config.AimbotVisible = bool.Parse(val);
                        break;
                    case "NoRecoil":
                        Config.NoRecoil = bool.Parse(val);
                        break;
                    case "UnlimitedAmmo":
                        Config.UnlimitedAmmo = bool.Parse(val);
                        break;
                    case "FastReload":
                        Config.FastReload = bool.Parse(val);
                        break;
                    case "speedint":
                        Config.speedint = bool.Parse(val);
                        break;
                    case "AimBotSmooth":
                        Config.AimBotSmooth = float.Parse(val);
                        break;
                    case "AimFov":
                        Config.AimFov = float.Parse(val);
                        break;
                    case "AimBotMaxDistance":
                        Config.AimBotMaxDistance = int.Parse(val);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Silent error - logging was causing file access conflicts
            }
        }
    }
}
