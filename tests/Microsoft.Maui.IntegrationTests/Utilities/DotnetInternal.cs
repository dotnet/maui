using System.Diagnostics;

namespace Microsoft.Maui.IntegrationTests
{
	public static class DotnetInternal
	{
		static readonly string DotnetTool = Path.Combine(TestEnvironment.GetMauiDirectory(), "bin", "dotnet", "dotnet");

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

			var buildOutput = Run("build", $"{buildArgs} -bl:\"{Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName)}\"", out int exitCode);
			if (exitCode != 0)
				TestContext.WriteLine(buildOutput);

			return exitCode == 0;
		}

		public static bool New(string shortName, string outputDirectory, string framework)
		{
			var runOutput = Run("new", $"{shortName} -o \"{outputDirectory}\" -f {framework}", out int exitcode, 60);
			if (exitcode != 0)
				TestContext.WriteLine(runOutput);

			return exitcode == 0;
		}


		public static string Run(string command, string args, out int exitCode, int timeoutInSeconds = 600)
		{
			var pinfo = new ProcessStartInfo()
			{
				FileName = DotnetTool,
				Arguments = $"{command} {args}",
			};
			pinfo.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";

			return ToolRunner.Run(pinfo, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
