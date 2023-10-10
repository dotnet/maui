using System.Diagnostics;
using System.IO;

namespace Microsoft.Maui.IntegrationTests
{
	public static class DotnetInternal
	{
		static readonly string DotnetRoot = Path.Combine(TestEnvironment.GetMauiDirectory(), "bin", "dotnet");
		static readonly string DotnetTool = Path.Combine(DotnetRoot, "dotnet");
		const int DEFAULT_TIMEOUT = 900;

		public static bool Build(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "", bool msbuildWarningsAsErrors = false)
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

			if (string.IsNullOrEmpty(binlogPath))
			{
				binlogPath = Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName);
			}

			if (msbuildWarningsAsErrors)
			{
				// We set WarnAsError to specifically cause *MSBuild* warnings to be errors (setting TreatWarningsAsErrors
				// affect only C# compiler warnings).
				buildArgs += " -warnaserror";

				// However, we need to ignore specific MSBuild warnings that are acceptable in these tests:
				var csWarningsToIgnore = new string[]
				{
					"NETSDK1201", // Details: "For projects targeting .NET 8.0 and higher, specifying a RuntimeIdentifier
								// will no longer produce a self contained app by default. To continue building
								// self-contained apps, set the SelfContained property to true or use the --self-contained
								// argument."
								// Justification: This warning isn't meaningful in this test scenario.
					"CS1591", // Details: "Missing XML comment for publicly visible type or member 'XYZ'"
							// Justification: It's OK for templates to have missing doc comments.
				};
				buildArgs += " " + string.Join(" ", csWarningsToIgnore.Select(csWarning => $"-p:nowarn={csWarning}"));
			}

			return Run("build", $"{buildArgs} -bl:\"{binlogPath}\"");
		}

		public static bool Publish(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "")
		{
			var binlogName = $"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
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

			if (string.IsNullOrEmpty(binlogPath))
			{
				binlogPath = Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName);
			}

			return Run("publish", $"{buildArgs} -bl:\"{binlogPath}\"");
		}

		public static bool New(string shortName, string outputDirectory, string framework = "")
		{
			var args = $"{shortName} -o \"{outputDirectory}\"";

			if (!string.IsNullOrEmpty(framework))
				args += $" -f {framework}";

			var output = RunForOutput("new", args, out int exitCode, timeoutInSeconds: 300);
			TestContext.WriteLine(output);
			return exitCode == 0;
		}

		public static bool Run(string command, string args, int timeoutinSeconds = DEFAULT_TIMEOUT)
		{
			var runOutput = RunForOutput(command, args, out int exitCode, timeoutinSeconds);
			TestContext.WriteLine($"Process exit code: {exitCode}");
			TestContext.WriteLine($"-------- Process output start --------");
			TestContext.WriteLine(runOutput);
			TestContext.WriteLine($"-------- Process output end --------");

			return exitCode == 0;
		}

		public static string RunForOutput(string command, string args, out int exitCode, int timeoutInSeconds = DEFAULT_TIMEOUT)
		{
			var pinfo = new ProcessStartInfo(DotnetTool, $"{command} {args}");
			pinfo.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";
			//Workaround: https://github.com/dotnet/linker/issues/3012
			pinfo.EnvironmentVariables["DOTNET_gcServer"] = "0";

			pinfo.EnvironmentVariables["DOTNET_ROOT"] = DotnetRoot;

			return ToolRunner.Run(pinfo, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
