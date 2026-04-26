using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cryptographic;

namespace KeyAuth
{
    public class api
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalAddAtom(string lpString);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalFindAtom(string lpString);

        public string name, ownerid, version, path, seed;

        public api(string name, string ownerid, string version, string path = null)
        {
            if (ownerid.Length != 10)
            {
                MessageBox.Show("Application not setup correctly. Please check your credentials.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                TerminateProcess(GetCurrentProcess(), 1);
            }
            this.name    = name;
            this.ownerid = ownerid;
            this.version = version;
            this.path    = path;
        }

        #region structures
        [DataContract]
        private class response_structure
        {
            [DataMember] public bool   success    { get; set; }
            [DataMember] public bool   newSession  { get; set; }
            [DataMember] public string sessionid   { get; set; }
            [DataMember] public string contents    { get; set; }
            [DataMember] public string response    { get; set; }
            [DataMember] public string message     { get; set; }
            [DataMember] public string ownerid     { get; set; }
            [DataMember] public string download    { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)] public user_data_structure info    { get; set; }
            [DataMember(IsRequired = false, EmitDefaultValue = false)] public app_data_structure  appinfo { get; set; }
            [DataMember] public List<msg>   messages { get; set; }
            [DataMember] public List<users> users    { get; set; }
            [DataMember(Name = "2fa", IsRequired = false, EmitDefaultValue = false)] public TwoFactorData twoFactor { get; set; }
        }

        public class msg   { public string message { get; set; } public string author { get; set; } public string timestamp { get; set; } }
        public class users { public string credential { get; set; } }

        [DataContract]
        private class user_data_structure
        {
            [DataMember] public string username   { get; set; }
            [DataMember] public string ip         { get; set; }
            [DataMember] public string hwid       { get; set; }
            [DataMember] public string createdate { get; set; }
            [DataMember] public string lastlogin  { get; set; }
            [DataMember] public List<Data> subscriptions { get; set; }
        }

        [DataContract]
        private class app_data_structure
        {
            [DataMember] public string numUsers         { get; set; }
            [DataMember] public string numOnlineUsers   { get; set; }
            [DataMember] public string numKeys          { get; set; }
            [DataMember] public string version          { get; set; }
            [DataMember] public string customerPanelLink { get; set; }
            [DataMember] public string downloadLink     { get; set; }
        }
        #endregion

        private static string sessionid, enckey;
        bool initialized;

        public async Task init()
        {
            var rng    = new Random();
            int length = rng.Next(5, 51);
            var sb     = new StringBuilder(length);
            for (int i = 0; i < length; i++) sb.Append((char)rng.Next(32, 127));
            seed = sb.ToString();
            checkAtom();

            var values = new NameValueCollection
            {
                ["type"]    = "init",
                ["ver"]     = version,
                ["hash"]    = checksum(Process.GetCurrentProcess().MainModule.FileName),
                ["name"]    = name,
                ["ownerid"] = ownerid
            };
            if (!string.IsNullOrEmpty(path))
            {
                values.Add("token", File.ReadAllText(path));
                values.Add("thash", TokenHash(path));
            }

            var response = await req(values);
            if (response == "KeyAuth_Invalid") { error("Application not found"); TerminateProcess(GetCurrentProcess(), 1); }

            var json = response_decoder.string_to_generic<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                load_response_struct(json);
                if (json.success) { sessionid = json.sessionid; initialized = true; }
                else if (json.message == "invalidver") app_data.downloadLink = json.download;
            }
            else TerminateProcess(GetCurrentProcess(), 1);
        }

        #pragma warning disable IDE0052
        private System.Threading.Timer atomTimer;
        #pragma warning restore IDE0052
        void checkAtom()
        {
            atomTimer = new System.Threading.Timer(_ =>
            {
                if (GlobalFindAtom(seed) == 0) TerminateProcess(GetCurrentProcess(), 1);
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public static string TokenHash(string tokenPath)
        {
            using var sha256 = SHA256.Create();
            using var s = File.OpenRead(tokenPath);
            return BitConverter.ToString(sha256.ComputeHash(s)).Replace("-", string.Empty);
        }

        public void CheckInit()
        {
            if (!initialized) { error("You must call init() first."); TerminateProcess(GetCurrentProcess(), 1); }
        }

        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            try { return dt.AddSeconds(unixtime).ToLocalTime(); }
            catch { return DateTime.MaxValue; }
        }

        public async Task login(string username, string pass, string code = null)
        {
            CheckInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var values = new NameValueCollection
            {
                ["type"]      = "login",
                ["username"]  = username,
                ["pass"]      = pass,
                ["hwid"]      = hwid,
                ["sessionid"] = sessionid,
                ["name"]      = name,
                ["ownerid"]   = ownerid,
                ["code"]      = code ?? null
            };
            var response = await req(values);
            var json     = response_decoder.string_to_generic<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                GlobalAddAtom(seed);
                GlobalAddAtom(ownerid);
                load_response_struct(json);
                if (json.success) load_user_data(json.info);
            }
            else TerminateProcess(GetCurrentProcess(), 1);
        }

        public async Task register(string username, string pass, string key, string email = "")
        {
            CheckInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var values = new NameValueCollection
            {
                ["type"]      = "register",
                ["username"]  = username,
                ["pass"]      = pass,
                ["key"]       = key,
                ["email"]     = email,
                ["hwid"]      = hwid,
                ["sessionid"] = sessionid,
                ["name"]      = name,
                ["ownerid"]   = ownerid
            };
            var response = await req(values);
            var json     = response_decoder.string_to_generic<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                GlobalAddAtom(seed); GlobalAddAtom(ownerid);
                load_response_struct(json);
                if (json.success) load_user_data(json.info);
            }
            else TerminateProcess(GetCurrentProcess(), 1);
        }

        public async Task license(string key, string code = null)
        {
            CheckInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var values = new NameValueCollection
            {
                ["type"]      = "license",
                ["key"]       = key,
                ["hwid"]      = hwid,
                ["sessionid"] = sessionid,
                ["name"]      = name,
                ["ownerid"]   = ownerid,
                ["code"]      = code ?? null
            };
            var response = await req(values);
            var json     = response_decoder.string_to_generic<response_structure>(response);
            if (json.ownerid == ownerid)
            {
                GlobalAddAtom(seed); GlobalAddAtom(ownerid);
                load_response_struct(json);
                if (json.success) load_user_data(json.info);
            }
            else TerminateProcess(GetCurrentProcess(), 1);
        }

        public async Task<bool> checkblack()
        {
            CheckInit();
            string hwid = WindowsIdentity.GetCurrent().User.Value;
            var values = new NameValueCollection
            {
                ["type"]      = "checkblacklist",
                ["hwid"]      = hwid,
                ["sessionid"] = sessionid,
                ["name"]      = name,
                ["ownerid"]   = ownerid
            };
            var response = await req(values);
            var json     = response_decoder.string_to_generic<response_structure>(response);
            if (json.ownerid == ownerid) { load_response_struct(json); return json.success; }
            TerminateProcess(GetCurrentProcess(), 1);
            return true;
        }

        public async Task log(string message)
        {
            CheckInit();
            var values = new NameValueCollection
            {
                ["type"]      = "log",
                ["pcuser"]    = Environment.UserName,
                ["message"]   = message,
                ["sessionid"] = sessionid,
                ["name"]      = name,
                ["ownerid"]   = ownerid
            };
            await req(values);
        }

        public static string checksum(string filename)
        {
            using var md = MD5.Create();
            using var fs = File.OpenRead(filename);
            return BitConverter.ToString(md.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
        }

        public static void error(string message)
        {
            string folder = "Logs", file = Path.Combine(folder, "ErrorLogs.txt");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (!File.Exists(file)) File.AppendAllText(file, DateTime.Now + " > Error log start\n");
            File.AppendAllText(file, DateTime.Now + $" > {message}\n");
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        private static async Task<string> req(NameValueCollection post_data)
        {
            try
            {
                if (FileCheck("keyauth.win"))
                {
                    error("File manipulation detected. Terminating.");
                    TerminateProcess(GetCurrentProcess(), 1);
                    return "";
                }

                var formData = post_data.AllKeys.Select(k => new KeyValuePair<string, string>(k, post_data[k])).ToList();
                var content  = new FormUrlEncodedContent(formData);
                var handler  = new HttpClientHandler
                {
                    Proxy = null,
                    ServerCertificateCustomValidationCallback = (req, cert, chain, err) => assertSSL(req, cert, chain, err)
                };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
                var httpResponse = await client.PostAsync("https://keyauth.win/api/1.3/", content);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    if (httpResponse.StatusCode == (HttpStatusCode)429)
                        error("You're connecting too fast. Slow down.");
                    else
                        error("Connection failure. Please try again.");
                    return "";
                }

                string raw = await httpResponse.Content.ReadAsStringAsync();
                var headers = new WebHeaderCollection();
                if (httpResponse.Headers.TryGetValues("x-signature-ed25519", out var sig))  headers["x-signature-ed25519"]  = sig.FirstOrDefault();
                if (httpResponse.Headers.TryGetValues("x-signature-timestamp", out var ts)) headers["x-signature-timestamp"] = ts.FirstOrDefault();
                sigCheck(raw, headers, post_data.Get(0));
                Logger.LogEvent(raw + "\n");
                return raw;
            }
            catch (Exception ex)
            {
                error("Connection failure: " + ex.Message);
                TerminateProcess(GetCurrentProcess(), 1);
                return "";
            }
        }

        private static bool FileCheck(string domain)
        {
            try
            {
                foreach (var addr in Dns.GetHostAddresses(domain))
                    if (IPAddress.IsLoopback(addr) || IsPrivateIP(addr)) return true;
                return false;
            }
            catch { return true; }
        }

        private static bool IsPrivateIP(IPAddress ip)
        {
            byte[] b = ip.GetAddressBytes();
            if (b[0] == 10) return true;
            if (b[0] == 172 && b[1] >= 16 && b[1] < 32) return true;
            if (b[0] == 192 && b[1] == 168) return true;
            return false;
        }

        private static bool assertSSL(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            if ((!cert.Issuer.Contains("Google Trust Services") && !cert.Issuer.Contains("Let's Encrypt")) || errors != SslPolicyErrors.None)
            {
                error("SSL assertion failed. Check network/firewall settings.");
                return false;
            }
            return true;
        }

        private static void sigCheck(string resp, WebHeaderCollection headers, string type)
        {
            if (type is "log" or "file" or "2faenable" or "2fadisable") return;
            try
            {
                string signature = headers["x-signature-ed25519"];
                string timestamp = headers["x-signature-timestamp"];
                if (!long.TryParse(timestamp, out long unixTs)) { error("Failed to parse server timestamp."); TerminateProcess(GetCurrentProcess(), 1); }
                if ((DateTime.UtcNow - DateTimeOffset.FromUnixTimeSeconds(unixTs).UtcDateTime).TotalSeconds > 20)
                { error("Date/Time not synced. Please sync your system clock."); TerminateProcess(GetCurrentProcess(), 1); }
                bool valid = Ed25519.CheckValid(
                    encryption.str_to_byte_arr(signature),
                    Encoding.Default.GetBytes(timestamp + resp),
                    encryption.str_to_byte_arr("5586b4bc69c7a4b487e4563a4cd96afd39140f919bd31cea7d1c6a1e8439422b"));
                if (!valid) { error("Signature checksum failed. Request may have been tampered."); TerminateProcess(GetCurrentProcess(), 1); }
            }
            catch { error("Signature checksum failed."); TerminateProcess(GetCurrentProcess(), 1); }
        }

        #region app_data
        public app_data_class app_data = new();
        public class app_data_class
        {
            public string numUsers { get; set; } public string numOnlineUsers { get; set; }
            public string numKeys  { get; set; } public string version        { get; set; }
            public string customerPanelLink { get; set; } public string downloadLink { get; set; }
        }
        private void load_app_data(app_data_structure d)
        {
            app_data.numUsers = d.numUsers; app_data.numOnlineUsers = d.numOnlineUsers;
            app_data.numKeys  = d.numKeys;  app_data.version = d.version;
            app_data.customerPanelLink = d.customerPanelLink;
        }
        #endregion

        #region user_data
        public user_data_class user_data = new();
        public class user_data_class
        {
            public string username { get; set; } public string ip { get; set; }
            public string hwid { get; set; } public string createdate { get; set; } public string lastlogin { get; set; }
            public List<Data> subscriptions { get; set; }
            public DateTime CreationDate  => api.UnixTimeToDateTime(long.Parse(createdate));
            public DateTime LastLoginDate => api.UnixTimeToDateTime(long.Parse(lastlogin));
        }
        public class Data
        {
            public string subscription { get; set; } public string expiry { get; set; }
            public string timeleft { get; set; } public string key { get; set; }
            public DateTime expiration => api.UnixTimeToDateTime(long.Parse(expiry));
        }
        private void load_user_data(user_data_structure d)
        {
            user_data.username = d.username; user_data.ip = d.ip; user_data.hwid = d.hwid;
            user_data.createdate = d.createdate; user_data.lastlogin = d.lastlogin;
            user_data.subscriptions = d.subscriptions;
        }
        #endregion

        [DataContract]
        private class TwoFactorData
        {
            [DataMember(Name = "secret_code")] public string SecretCode { get; set; }
            [DataMember(Name = "QRCode")]      public string QRCode     { get; set; }
        }

        #region response
        public response_class response = new();
        public class response_class { public bool success { get; set; } public string message { get; set; } }
        private void load_response_struct(response_structure d) { response.success = d.success; response.message = d.message; }
        #endregion

        private json_wrapper response_decoder = new json_wrapper(new response_structure());
    }

    public static class Logger
    {
        public static bool IsLoggingEnabled { get; set; } = false;
        public static void LogEvent(string content)
        {
            if (!IsLoggingEnabled) return;
            string exeName  = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
            string logDir   = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "KeyAuth", "debug", exeName);
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            string logFile  = Path.Combine(logDir, $"{DateTime.Now:MMM_dd_yyyy}_logs.txt");
            try
            {
                foreach (var f in new[] { "sessionid","ownerid","app","version","fileid","webhooks","nonce" })
                    content = Regex.Replace(content, $"\"{f}\":\"[^\"]*\"", $"\"{f}\":\"REDACTED\"");
                File.AppendText(logFile).WriteLine($"[{DateTime.Now}] {content}");
            }
            catch { }
        }
    }

    public static class encryption
    {
        [DllImport("kernel32.dll")] private static extern bool TerminateProcess(IntPtr h, uint c);
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();

        public static string byte_arr_to_str(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] str_to_byte_arr(string hex)
        {
            try
            {
                int n = hex.Length;
                byte[] bytes = new byte[n / 2];
                for (int i = 0; i < n; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch { api.error("Session ended. Please reopen the program."); TerminateProcess(GetCurrentProcess(), 1); return null; }
        }

        public static string iv_key() => Guid.NewGuid().ToString().Substring(0, 16);
    }

    public class json_wrapper
    {
        private readonly DataContractJsonSerializer serializer;
        private readonly object current_object;
        public static bool is_serializable(Type t) => t.IsSerializable || t.IsDefined(typeof(DataContractAttribute), true);
        public json_wrapper(object obj)
        {
            current_object = obj;
            var t = obj.GetType();
            serializer = new DataContractJsonSerializer(t);
            if (!is_serializable(t)) throw new Exception($"Object {obj} is not serializable");
        }
        public object string_to_object(string json)
        {
            using var ms = new MemoryStream(Encoding.Default.GetBytes(json));
            return serializer.ReadObject(ms);
        }
        public T string_to_generic<T>(string json) => (T)string_to_object(json);
    }
}
