using System.CommandLine;

namespace Microsoft.Maui.Cli;

/// <summary>
/// Common options shared across all commands.
/// These follow the conventions established by dotnet run for .NET MAUI.
/// </summary>
internal static class CommonOptions
{
    /// <summary>
    /// Enables verbose output for debugging.
    /// </summary>
    public static readonly Option<bool> VerboseOption = new("--verbose")
    {
        Description = "Enable verbose output for debugging"
    };

    /// <summary>
    /// Target framework (e.g., net10.0-android, net10.0-ios).
    /// </summary>
    public static readonly Option<string?> FrameworkOption = new("-f", "--framework")
    {
        Description = "Target framework (e.g., net10.0-android, net10.0-ios)"
    };

    /// <summary>
    /// Target device identifier (from --list-devices).
    /// </summary>
    public static readonly Option<string?> DeviceOption = new("-d", "--device")
    {
        Description = "Target device identifier"
    };

    /// <summary>
    /// Path to the .NET MAUI project.
    /// </summary>
    public static readonly Option<string?> ProjectOption = new("-p", "--project")
    {
        Description = "Path to the .NET MAUI project (default: current directory)"
    };
}
