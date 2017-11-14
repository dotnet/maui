using System;
using System.Diagnostics;

namespace Xamarin.Forms.Platform.GTK.Helpers
{
    public enum GTKPlatform
    {
        Linux,
        MacOS,
        Windows
    }

    public class PlatformHelper
    {
        public static GTKPlatform GetGTKPlatform()
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return GTKPlatform.Windows;
                case PlatformID.Unix:
                    if (GetCommandExecutionOutput("uname", string.Empty) == "Darwin\n")
                    {
                        return GTKPlatform.MacOS;
                    }
                    else
                    {
                        return GTKPlatform.Linux;
                    }
                case PlatformID.MacOSX:
                    return GTKPlatform.MacOS;
                default:
                    return GTKPlatform.Windows;

            }
        }

        internal static string GetCommandExecutionOutput(string command, string arguments)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();

            if (string.IsNullOrEmpty(output))
            {
                output = process.StandardError.ReadToEnd();
            }

            return output;
        }
    }
}