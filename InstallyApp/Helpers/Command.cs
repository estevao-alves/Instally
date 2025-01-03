using System.Net;
using System.Text;
using System.Threading;

namespace InstallyApp.Utils.Functions
{
    public static class Command
    {
        public static CancellationTokenSource CancellationTokenSource { get; set; } = new();

        public static string wingetExe = @"%LOCALAPPDATA%\Microsoft\WindowsApps\winget.exe";

        public static async Task<bool> Download(string url, string pathDest)
        {
            try
            {
                WebClient client = new();
                client.DownloadFile(url, pathDest);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<string> Execute(string fileName, string arguments)
        {
            try
            {
                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.Default
                    }
                };

                StringBuilder outputBuilder = new();
                StringBuilder errorBuilder = new();

                p.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                        // You can log or process the output here in real-time
                        if (App.Debug is not null) App.Debug.CreateInfo(args.Data);
                    }
                };

                p.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                        // You can also log error output if necessary
                        if (App.Debug is not null) App.Debug.CreateInfo("Error: " + args.Data);
                    }
                };

                p.Start();

                // Begin asynchronously reading the output
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                await p.WaitForExitAsync();

                if (p.ExitCode != 0 && App.Debug is not null)
                {
                    App.Debug.CreateInfo("Process exited with errors:\n" + errorBuilder.ToString());
                }

                return outputBuilder.ToString();
            }
            catch (Exception ex)
            {
                if (App.Debug is not null) App.Debug.CreateInfo("Error executing a command:\nDetails: " + ex.Message);
                return "Error executing a command";
            }
        }
    }
}