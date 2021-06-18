using System;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen
{
	[Generator]
	public class CodeBehindGenerator : ISourceGenerator
	{
		static string[] _csharpKeywords = new[] {
			"abstract", "as",
			"base", "bool", "break", "byte",
			"case", "catch", "char", "checked", "class", "const", "continue",
			"decimal", "default", "delegate", "do", "double",
			"else", "enum", "event", "explicit", "extern",
			"false", "finally", "fixed", "float", "for", "foreach",
			"goto",
			"if", "implicit", "in", "int", "interface", "internal", "is",
			"lock", "long", "namespace", "new", "null",
			"object", "operator", "out", "override",
			"params", "private", "protected", "public",
			"readonly", "ref", "return",
			"sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
			"this", "throw", "true", "try", "typeof",
			"uint", "ulong", "unchecked", "unsafe", "ushort", "using",
			"virtual", "void", "volatile",
			"while",
		};

		public void Initialize(GeneratorInitializationContext context)
		{
			//#if DEBUG
			//if (!Debugger.IsAttached)
			//	Debugger.Launch();
			//#endif
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.targetframework", out var targetFramework))
				return;

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

			var containsSplash = false;
			foreach (var file in context.AdditionalFiles)
			{
				var options = context.AnalyzerConfigOptions.GetOptions(file);
				if (options.TryGetValue("build_metadata.AdditionalFiles.IsMauiSplashScreen", out var isMauiSplashScreen) && bool.TryParse(isMauiSplashScreen, out var isSplash) && isSplash)
				{
					containsSplash = true;
					break;
				}
			}

			if (targetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateAndroidSource(context, rootNamespace, containsSplash);
				var name = "Android.sg.cs";
				context.AddSource(name, SourceText.From(code, Encoding.UTF8));
			}
			else if (targetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource(context, rootNamespace);
				var name = "iOS.sg.cs";
				context.AddSource(name, SourceText.From(code, Encoding.UTF8));
			}
			else if (targetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource(context, rootNamespace);
				var name = "MacCatalyst.sg.cs";
				context.AddSource(name, SourceText.From(code, Encoding.UTF8));
			}
		}

		string GenerateAndroidSource(GeneratorExecutionContext context, string? rootNamespace, bool containsSplash)
		{
			var ns = rootNamespace;
			var startupName = "Startup";
			var appName = "MainApplication";
			var activityName = "MainActivity";
			var splash = containsSplash ? @"Theme = ""@style/Maui.SplashTheme""," : "";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_APPLICATION_GENERATION
namespace " + ns + @"
{
	[global::Android.App.Application]
	partial class " + appName + @" : global::Microsoft.Maui.MauiApplication<global::" + ns + @"." + startupName + @">
	{
		public " + appName + @"(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_ACTIVITY_GENERATION
namespace " + ns + @"
{
	[global::Android.App.Activity(
		" + splash + @"
		MainLauncher = true,
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation |
			global::Android.Content.PM.ConfigChanges.UiMode |
			global::Android.Content.PM.ConfigChanges.ScreenLayout |
			global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
	partial class " + activityName + @" : global::Microsoft.Maui.MauiAppCompatActivity
	{
	}
}
#endif
";
		}

		string GenerateIosSource(GeneratorExecutionContext context, string? rootNamespace)
		{
			var ns = rootNamespace;
			var startupName = "Startup";
			var delegateName = "AppDelegate";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_PROGRAM_GENERATION
namespace " + ns + @"
{
	partial class Program
	{
		static void Main(string[] args)
		{
			global::UIKit.UIApplication.Main(args, null, nameof(global::" + ns + @"." + delegateName + @"));
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_APP_DELEGATE_GENERATION
namespace " + ns + @"
{
	[global::Foundation.Register(nameof(" + delegateName + @"))]
	partial class " + delegateName + @" : global::Microsoft.Maui.MauiUIApplicationDelegate<global::" + ns + @"." + startupName + @">
	{
	}
}
#endif
";
		}
	}
}
