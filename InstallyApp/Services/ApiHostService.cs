using System.IO;
using System.Net.Http;

public class ApiHostService
{
    private Process? _apiProcess;

    public async Task StartAsync()
    {
        string apiPath = Path.Combine(
            AppContext.BaseDirectory,
            OperatingSystem.IsWindows()
                ? "InstallyAPI.exe"
                : "InstallyAPI"
        );

        _apiProcess = Process.Start(new ProcessStartInfo
        {
            FileName = apiPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(apiPath)
        });

        using HttpClient client = new();

        for (int i = 0; i < 25; i++)
        {
            try
            {
                await client.GetAsync("http://localhost:23842");
                return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        throw new Exception("API failed to start.");
    }

    public void Stop()
    {
        if (_apiProcess is { HasExited: false })
            _apiProcess.Kill(true);
    }
}