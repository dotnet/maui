using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests
{
	public static class DotnetInternal
	{
		static readonly string DotnetTool = Path.Combine(TestEnvironment.GetMauiDirectory(), "bin", "dotnet", "dotnet");
		const int DEFAULT_TIMEOUT = 600;

		public static bool Build(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null)
		{
			var binlogName = $"build-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
			var buildArgs = $"\"{projectFile}\" -c {config}";

			if (!string.IsNullOrEmpty(target))
			{
				binlogName = $"{target}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
				buildArgs += $" -t:{target}";
			}

			if (!string.IsNullOrEmpty(framework))
				buildArgs += $" -f:{framework}";

			if (properties != null)
			{
				foreach (var p in properties)
				{
					buildArgs += $" -p:{p}";
				}
			}

			return Run("build", $"{buildArgs} -bl:\"{Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName)}\"");
		}

		public static bool New(string shortName, string outputDirectory, string framework)
		{
			return Run("new", $"{shortName} -o \"{outputDirectory}\" -f {framework}", timeoutinSeconds: 60);
		}

		public static bool Run(string command, string args, int timeoutinSeconds = DEFAULT_TIMEOUT)
		{
			var runOutput = RunForOutput(command, args, out int exitCode, timeoutinSeconds);
			if (exitCode != 0)
				TestContext.WriteLine(runOutput);

			return exitCode == 0;
		}

		public static string RunForOutput(string command, string args, out int exitCode, int timeoutInSeconds = DEFAULT_TIMEOUT)
		{
			var pinfo = new ProcessStartInfo(DotnetTool, $"{command} {args}");
			pinfo.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";

			return ToolRunner.Run(pinfo, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
