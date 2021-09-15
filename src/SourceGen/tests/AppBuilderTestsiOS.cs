using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.TestUtils.SourceGen;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.SourceGen.Tests
{
	public class AppBuilderTestsiOS : BaseSourceGeneratorTests<AppBuilderGenerator>
	{
		public AppBuilderTestsiOS(ITestOutputHelper output) : base(output)
		{
			Generator.AddReferences("Microsoft.Maui");

			Generator.AddMSBuildProperty("TargetPlatformIdentifier", "ios");
			Generator.AddMSBuildProperty("OutputType", "Exe");
			Generator.AddMSBuildProperty("UseMaui", "true");
		}

		[Fact]
		public void Generates_AppDelegate_Main()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;

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

			RunGenerator();

			Compilation.AssertGeneratedContent("Register(nameof(AppDelegate))]");
			Compilation.AssertGeneratedContent("static void Main");
			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
		}

		[Fact]
		public void Generates_AppDelegate()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;

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

	public class Program
	{
		static void Main(string[] args)
		{
		}
	}
}
");

			RunGenerator();

			Compilation.AssertGeneratedContent("Register(nameof(AppDelegate))]");
			Compilation.AssertNotGeneratedContent("static void Main");
			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
		}

		[Fact]
		public void Generates_PartialAppDelegate()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;

namespace MyApp
{
	public partial class MauiProgram
	{
		public static MauiApp CreateApp()
		{
			var builder = new MauiAppBuilder();
			return builder.Build();
		}
	}

	public partial class MyAppDelegate : global::Microsoft.Maui.MauiUIApplicationDelegate
	{
	}

	public class Program
	{
		static void Main(string[] args)
		{
		}
	}
}
");

			RunGenerator();

			Compilation.AssertNotGeneratedContent("static void Main");
			Compilation.AssertGeneratedContent("partial class MyAppDelegate");
			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
		}


		[Fact]
		public void Generates_PartialAppDelegate_Main()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;

namespace MyApp
{
	public partial class MauiProgram
	{
		public static MauiApp CreateApp()
		{
			var builder = new MauiAppBuilder();
			return builder.Build();
		}
	}

	public partial class MyAppDelegate : global::Microsoft.Maui.MauiUIApplicationDelegate
	{
	}
}
");

			RunGenerator();

			Compilation.AssertGeneratedContent("static void Main");
			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
			Compilation.AssertGeneratedContent("partial class MyAppDelegate");
		}

		[Fact]
		public void Generates_Nothing()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;

namespace MyApp
{
	public partial class MauiProgram
	{
		public static MauiApp CreateApp()
		{
			var builder = new MauiAppBuilder();
			return builder.Build();
		}
	}

	public partial class MyAppDelegate : global::Microsoft.Maui.MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}

	public class Program
	{
		static void Main(string[] args)
		{
		}
	}
}
");

			RunGenerator();

			Compilation.AssertNotGeneratedContent("static void Main");
			Compilation.AssertNotGeneratedContent("partial class MyAppDelegate");
			Compilation.AssertNotGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
		}
	}
}
