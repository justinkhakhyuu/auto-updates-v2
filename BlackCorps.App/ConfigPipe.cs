using System.IO.Pipes;
using System.Text;

namespace BlackCorps.App;

internal static class ConfigPipe
{
    private const string PipeName = "BlackCorps_Config";
    private static NamedPipeClientStream? _client;

    public static void Initialize()
    {
        try
        {
            _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            _client.Connect(1000); // 1 second timeout, don't block UI
        }
        catch (Exception ex)
        {
            _client = null;
        }
    }

    public static void Send(string key, bool value)
    {
        try
        {
            if (_client == null || !_client.IsConnected)
            {
                _client?.Dispose();
                _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                _client.Connect(500);
            }

            if (_client?.IsConnected == true)
            {
                string msg = $"{key}={value}\n";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _client.Write(data, 0, data.Length);
                _client.Flush();
            }
        }
        catch { }
    }

    public static void SendFloat(string key, float value)
    {
        try
        {
            if (_client == null || !_client.IsConnected)
            {
                _client?.Dispose();
                _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                _client.Connect(500);
            }

            if (_client?.IsConnected == true)
            {
                string msg = $"{key}={value}\n";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _client.Write(data, 0, data.Length);
                _client.Flush();
            }
        }
        catch { }
    }

    public static void SendInt(string key, int value)
    {
        try
        {
            if (_client == null || !_client.IsConnected)
            {
                _client?.Dispose();
                _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                _client.Connect(500);
            }

            if (_client?.IsConnected == true)
            {
                string msg = $"{key}={value}\n";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                _client.Write(data, 0, data.Length);
                _client.Flush();
            }
        }
        catch { }
    }

    public static void Close()
    {
        _client?.Dispose();
        _client = null;
    }
}
