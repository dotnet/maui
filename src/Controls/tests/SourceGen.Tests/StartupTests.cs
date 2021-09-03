using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Controls.SourceGen.Tests
{
	public class StartupTests : BaseSourceGeneratorTests<AppBuilderGenerator>
	{
		public StartupTests(ITestOutputHelper output)
			: base(output, "Microsoft.Maui", "Microsoft.Maui.Controls")
		{

			Generator.AddMSBuildProperty("TargetPlatformIdentifier", "ios");
			Generator.AddMSBuildProperty("OutputType", "Exe");

			// Add our startup so the startup generators will even run
			this.Generator.AddSource(@"
using Microsoft.Maui;

namespace MyApp
{
	public class MauiProgram
	{
		public static MauiApp CreateApp()
		{
			var builder = new MauiAppBuilder();
			return builder.Build();
		}
	}
}
");
		}

		[Fact]
		public void AppDelegateCreated()
		{
			
			this.RunGenerator();
			this.Compilation.AssertContent("AppDelegate");
			
		}
	}
}
