using System.Drawing;

using System.Media;

using System.Reflection;

using System.Runtime.CompilerServices;

using System.Windows.Forms;

using static Guna.UI2.Native.WinApi;



namespace AotForms

{

    internal static class Config

    {

        internal static bool StreamerMode = false;

        internal static bool ESPLineRGB = false;

        internal static bool ESPLineRGBGradient = false;

        internal static float AimBotSmooth = 16f;

        internal static AimBotType AimBotType;

        internal static bool enableAimBot = false;

        internal static bool UpPlayer = false;

        internal static bool AimbotVisible = false;

        internal static bool ShakeUltra = false;

        internal static bool CameraD = false;

        internal static bool Aimkill = false;



        internal static int AIMBOTDELAY = 5;

        internal static bool Tele = false;

        internal static bool FOV360 = false;



        //internal static bool JoystickSpeed = false;



        // Enemy Pull Configuration

        public static bool EnemyPullEnabled = false;

        public static float EnemyPullStrengthNormal = 5f;

        public static int EnemyPullMaxDistanceNormal = 200;

        public static int EnemyPullTickMsNormal = 8;



        public static bool teli = false;



        // Inside your Config class

        public static bool teliv2 = false;



        // Bone Selection Settings

        public static bool AimAtHead = true;

        public static bool AimAtBody = false;

        public static bool AimAtHip = false;

        public static bool spinbot = false;

        public static float SpinSpeed = 10.0f; // Default speed







        public static int EnemyPullTickMs = 6;

        public static float EnemyPullMaxDistance = 300f; // Adjust as needed

        public static float Aimfov = 200f;               // Adjust as needed

        public static bool IgnoreKnocked = true;



        // Bone Selection



        // Aimbot Key (e.g., Right Mouse Button = 0x02)

        public static bool PullXZY = false;

        public static float Xyz = 1.4f;

 

        public static int EnemyPullKey = 0x14; // Default to CAPS LOCK (0x14)



        internal static bool UnlimitedAmmo = false;

        internal static bool speedint = false;

        internal static bool ESPInformation = false;

        internal static bool espweapon = false;

        internal static bool AimBotRage = false;

        internal static bool SilentAim = false;

        internal static bool spawnkill = false;

        internal static bool FastReload = false;

        internal static bool SILENT = false;

        internal static Color MiniMapColor = Color.White;

        internal static Keys AimbotKey = Keys.LButton;

        internal static bool EspUp = true;

        internal static bool EspBottom = false;

        internal static bool EspMiddle = false;

        internal static int AimBotMaxDistance = 200;

        internal static float test = 10;

        public static float GlowRadius = 15;

        internal static bool UpdateEntities = false;

        internal static bool NoRecoil = false;

        internal static bool NoCache = false;

        internal static bool ESPShuriken = false;

        internal static bool ESPShurikenRotate = false;

        internal static bool EnemyCount = false;

        internal static bool ESPInfoBox = false;



        internal static TargetingMode TargetingMode  = TargetingMode.ClosestToCrosshair;

        internal static TargetingMode TargetingMode1 = TargetingMode.Target360;

        internal static TargetingMode TargetingMode2 = TargetingMode.ClosestToPlayer;

        internal static TargetingMode TargetingMode3 = TargetingMode.LowestHealth;

        internal static bool ESPDistance = false;

        internal static bool ESPLine = false;

        internal static Color ESPLineColor = Color.White;

        internal static Color ESPTargetAimColor = Color.White;

        internal static float AimFov = 100f;

        internal static bool StreamMode = false;

        internal static Color ESPDistanceColor = Color.White;

        internal static Color NameCheat = Color.Cyan;

        internal static bool ESPBox = false;

        internal static bool EspWeaponIcon = false;

        internal static Color ESPBoxColor = Color.White;

        internal static Color EnemyCountColor = Color.Yellow;

        internal static bool ESPName = false;

        internal static Color ESPNameColor = Color.White;

        internal static bool ESPHealth = false;

        internal static bool ESPHealthText = false;

        internal static Color ESPHealthColor = Color.Lime;

        internal static bool ESPSkeleton = false;  

        internal static Color ESPSkeletonColor = Color.White;

        internal static bool FOVEnabled = false;

        internal static bool Minimap = false;

        internal static bool CrosshairEnabled = false;

        internal static Color CrosshairColor = Color.White;

        internal static float CrosshairSize = 15f;

        public static float CrosshairRotationSpeed = 2f; // Default speed

        internal static Color FOVColor = Color.White;

        internal static LinePosition ESPLinePosition = LinePosition.Top;

        internal static Color ESPCornerBoxColor = Color.White;

        internal static float cameraVal = 1.0f;

        internal static float speedVal = 1.0f;

        internal static float visionVal = 3.141592741f;

        internal static bool ESPBox2 = false;

        internal static bool ESPFillBox = false;

        internal static Color ESPFillBoxColor = Color.White;

        internal static float test1 = 0.01f;

        internal static int espran = 150;

        public static float TeleportRange = 10f;

        internal static bool FixEsp = false;



        internal static bool Aimfovc = false;

        internal static Color Aimfovcolor = Color.White;



        internal static bool linetargetclose = false;







        public static float ESPBoxThickness = 1.0f;









        internal static bool sound = false;

        public static void Notif()

        {





            if (!sound)

            {

                // Replace "YourNamespace.YourMP3File.mp3" with the correct namespace and file name

                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Client.clicksound.wav");



                if (stream != null)

                {

                    using (SoundPlayer player = new SoundPlayer(stream))

                    {

                        player.Play();

                    }

                }

                else

                {



                }

            }



            else

            {





            }

        }



    }

    public enum TargetingMode

    {

        ClosestToCrosshair,

        Target360,

        ClosestToPlayer,

        LowestHealth,

    }

    public enum AimBotType

    {

        Rage,

        Visible,

        Silent360,

        SilentLite,

        

    }

    public enum LinePosition

    {

        Top,

        Center,

        Bottom

    }

   

}

