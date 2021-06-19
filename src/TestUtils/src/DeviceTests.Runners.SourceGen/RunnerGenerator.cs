using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	abstract class RunnerGenerator
	{
		protected RunnerGenerator(GeneratorExecutionContext context, string targetFramework)
		{
			Context = context;

			TargetFramework = targetFramework;

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ApplicationId", out var applicationId);
			ApplicationId = applicationId ?? throw new Exception("ApplicationId needs to be set.");

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ApplicationTitle", out var applicationTitle);
			ApplicationTitle = applicationTitle ?? "Tests";

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
			RootNamespace = rootNamespace ?? "TestRunnerNamespace";

			ContainsSplashScreen = false;
			foreach (var file in context.AdditionalFiles)
			{
				var options = context.AnalyzerConfigOptions.GetOptions(file);
				if (options.TryGetValue("build_metadata.AdditionalFiles.IsMauiSplashScreen", out var isMauiSplashScreen) && bool.TryParse(isMauiSplashScreen, out var isSplash) && isSplash)
				{
					ContainsSplashScreen = true;
					break;
				}
			}

#if DEBUG
			DebugGeneratorOutputPath = Path.Combine(Path.GetTempPath(), "Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen");
#endif
		}

		public GeneratorExecutionContext Context { get; }

		public string TargetFramework { get; }

		public string RootNamespace { get; }

		public string ApplicationId { get; }

		public string ApplicationTitle { get; }

		public bool ContainsSplashScreen { get; }

#if DEBUG
		public string DebugGeneratorOutputPath { get; }
#endif

		public abstract void Generate();

		protected void AddSource(string filename, string contents)
		{
#if DEBUG
			if (!Directory.Exists(DebugGeneratorOutputPath))
				Directory.CreateDirectory(DebugGeneratorOutputPath);

			var path = Path.Combine(DebugGeneratorOutputPath, filename);
			File.WriteAllText(path, contents);
#endif

			Context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
		}
	}
}