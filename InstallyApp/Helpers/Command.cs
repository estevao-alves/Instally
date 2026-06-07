using System.Net;
using System.Text;
using System.Threading;

namespace InstallyApp.Utils.Functions
{
    public class CommandResult
    {
        public string Command { get; set; } = "";
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
        public int ExitCode { get; set; }
    }
    
    public static class Command
    {
        public static async Task<CommandResult> Execute(string fileName, string arguments)
        {
            StringBuilder outputBuilder = new();
            StringBuilder errorBuilder = new();
            Process? process = null;
            
            try
            {
                process = new Process()
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

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        outputBuilder.AppendLine(args.Data);
                        // You can log or process the output here in real-time
                        if (App.Debug is not null) App.Debug.CreateInfo(args.Data);
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        errorBuilder.AppendLine(args.Data);
                        // You can also log error output if necessary
                        if (App.Debug is not null) App.Debug.CreateInfo("Error: " + args.Data);
                    }
                };

                process.Start();

                // Begin asynchronously reading the output
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0 && App.Debug is not null)
                {
                    App.Debug.CreateInfo("Process exited with errors:\n" + errorBuilder);
                }

                return new CommandResult
                {
                    Command = $"$  {fileName} {arguments}",
                    Output = RemoveEmptyLines(outputBuilder.ToString()),
                    Error = RemoveEmptyLines(errorBuilder.ToString()),
                    ExitCode = process.ExitCode
                };
            }
            catch (Exception ex)
            {
                if (App.Debug is not null) App.Debug.CreateInfo("Error executing a command:\nDetails: " + ex.Message);
                
                return new CommandResult
                {
                    Command = $"$  {fileName} {arguments}",
                    Output = outputBuilder.ToString(),
                    Error = ex.Message,
                    ExitCode = -1
                };
            }
        }
        
        private static string RemoveEmptyLines(string text)
        {
            return string.Join(
                Environment.NewLine,
                text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
            );
        }
    }
}