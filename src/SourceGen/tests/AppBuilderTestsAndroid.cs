using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.TestUtils.SourceGen;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.SourceGen.Tests
{
	public class AppBuilderTestsAndroid : BaseSourceGeneratorTests<AppBuilderGenerator>
	{
		const string platformIdentifier = "android";

		void AddPlatformReference(string assembly)
			=> Generator.AddReference(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Core", "src", "bin", Configuration, $"net6.0-{platformIdentifier}", assembly + ".dll"));

		public AppBuilderTestsAndroid(ITestOutputHelper output) : base(output)
		{
			AddPlatformReference("Microsoft.Maui");

			Generator.AddMSBuildProperty("EnableMauiAppBuilderSourceGen", "true");
			Generator.AddMSBuildProperty("TargetPlatformIdentifier", "android");
			Generator.AddMSBuildProperty("OutputType", "Exe");
			Generator.AddMSBuildProperty("UseMaui", "true");
		}

		[Fact]
		public void Generates_MainActivity()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using System;
using Android.App;
using Android.Runtime;

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

	[Application]
	public class MainApplication : Microsoft.Maui.MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
");

			RunGenerator();

			Compilation.AssertGeneratedContent("MainLauncher = true");
			Compilation.AssertNotGeneratedContent("[global::Android.App.Application]");
		}

		[Fact]
		public void Generates_MainActivity_Application()
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

			Compilation.AssertGeneratedContent("MainLauncher = true");
			Compilation.AssertGeneratedContent("[global::Android.App.Application]");
			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
		}


		[Fact]
		public void Generates_PartialApplication()
		{
			// Add our startup so the startup generators will even run
			Generator.AddSource(@"
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using System;
using Android.App;
using Android.Runtime;
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

	[Application]
	public partial class MainApplication : Microsoft.Maui.MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}
");

			RunGenerator();

			Compilation.AssertGeneratedContent("protected override global::Microsoft.Maui.MauiApp CreateMauiApp()");
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
using System;
using Android.App;
using Android.Runtime;
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

	[Application]
	public class MainApplication : Microsoft.Maui.MauiApplication
	{
		public MainApplication(IntPtr handle, JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}

	[global::Android.App.Activity(Label = ""Test"", MainLauncher = true)]
	public partial class MainActivity : global::Microsoft.Maui.MauiAppCompatActivity
	{
	}
");

			RunGenerator();

			Compilation.AssertNotGeneratedContent("MainLauncher = true");
			Compilation.AssertNotGeneratedContent("[global::Android.App.Application]");
		}
	}
}
