using System.Diagnostics;
using System.IO;

namespace Microsoft.Maui.IntegrationTests
{
	public static class DotnetInternal
	{
		static readonly string DotnetRoot = Path.Combine(TestEnvironment.GetMauiDirectory(), ".dotnet");
		static readonly string DotnetTool = Path.Combine(DotnetRoot, "dotnet");
		const int DEFAULT_TIMEOUT = 1800;

		private static string ConstructBuildArgs(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "", string runtimeIdentifier = "", bool isPublishing = false)
		{
			var buildArgs = $"\"{projectFile}\" -c {config}";

			if (!string.IsNullOrEmpty(target))
				buildArgs += $" -t:{target}";

			if (!string.IsNullOrEmpty(framework))
				buildArgs += $" -f:{framework}";

			if (!string.IsNullOrEmpty(runtimeIdentifier))
				buildArgs += $" -r:{runtimeIdentifier}";

			if (properties != null)
			{
				foreach (var p in properties)
				{
					buildArgs += $" -p:{p}";
				}
			}

			if (string.IsNullOrEmpty(binlogPath))
			{
				var binlogPrefix = string.Empty;
				if (!string.IsNullOrEmpty(target))
					binlogPrefix = target;
				else
					binlogPrefix = isPublishing ? "publish" : "build";

				var binlogName = $"{binlogPrefix}-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
				binlogPath = Path.Combine(Path.GetDirectoryName(projectFile) ?? "", binlogName);
			}
			buildArgs += $" -bl:\"{binlogPath}\"";

			return buildArgs;
		}

		public static bool Build(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "", bool msbuildWarningsAsErrors = false, string runtimeIdentifier = "",
			string[]? warningsToIgnore = null)
		{
			var buildArgs = ConstructBuildArgs(projectFile, config, target, framework, properties, binlogPath, runtimeIdentifier, false);

			if (msbuildWarningsAsErrors)
			{
				// We set WarnAsError to specifically cause *MSBuild* warnings to be errors (setting TreatWarningsAsErrors
				// affect only C# compiler warnings).
				buildArgs += " -warnaserror";

				// However, we need to ignore specific MSBuild warnings that are acceptable in these tests:
				var csWarningsToIgnore = new List<string>
				{
					"NETSDK1201", // Details: "For projects targeting .NET 8.0 and higher, specifying a RuntimeIdentifier
								// will no longer produce a self contained app by default. To continue building
								// self-contained apps, set the SelfContained property to true or use the --self-contained
								// argument."
								// Justification: This warning isn't meaningful in this test scenario.
					"CS1591", // Details: "Missing XML comment for publicly visible type or member 'XYZ'"
							// Justification: It's OK for templates to have missing doc comments.
				};

				if (warningsToIgnore?.Length > 0)
				{
					csWarningsToIgnore.AddRange(warningsToIgnore);
				}

				var csWarnings = string.Join("%3B", csWarningsToIgnore);
				buildArgs += $" -p:nowarn=\"{csWarnings}\"";
			}

			return Run("build", $"{buildArgs}");
		}

		public static bool Publish(string projectFile, string config, string target = "", string framework = "", IEnumerable<string>? properties = null, string binlogPath = "", string runtimeIdentifier = "")
		{
			var buildArgs = ConstructBuildArgs(projectFile, config, target, framework, properties, binlogPath, runtimeIdentifier, true);
			return Run("publish", $"{buildArgs}");
		}

		public static bool New(string shortName, string outputDirectory, string framework = "", string? additionalDotNetNewParams = null)
		{
			var args = $"{shortName} -o \"{outputDirectory}\"";

			if (!string.IsNullOrEmpty(framework))
			{
				args += $" -f {framework}";
			}

			args += $" {additionalDotNetNewParams}";

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
			TestContext.WriteLine($"Running: '{DotnetTool}' with '{command}'");
			TestContext.WriteLine($"Args list: {args}");
			var pinfo = new ProcessStartInfo(DotnetTool, $"{command} {args}");
			pinfo.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"] = "0";
			pinfo.EnvironmentVariables["DOTNET_ROOT"] = DotnetRoot;

			return ToolRunner.Run(pinfo, out exitCode, timeoutInSeconds: timeoutInSeconds);
		}

	}
}
