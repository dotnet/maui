using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.SourceGen
{
	[Generator]
	public class AppBuilderGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new AppStartupSyntaxReceiver());
		}

		static bool HasCreateMauiAppOverride(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
		{
			foreach (var m in classDeclarationSyntax.Members)
			{
				if (m is MethodDeclarationSyntax omds)
				{
					if (omds.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
					{
						var retType = context.GetReturnType(omds);
						if (retType?.Equals("Microsoft.Maui.MauiApp") ?? false)
							if (omds.Identifier.Text == "CreateMauiApp")
								return true;
					}
				}
			}

			return false;
		}

		class MaciOSMauiAppBuilderReceiver
		{
			public bool HasMainMethod { get; set; } = false;
			public bool HasAppDelegateSubclass { get; set; } = false;

			public TypeInfoParts? PartialAppDelegateSubclass { get; set; } = null;

			public bool PartialAppDelegateOverridesCreate { get; set; } = false;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
			{
				// Look for a Main method
				if (!HasMainMethod
					&& syntaxNode is MethodDeclarationSyntax method
					&& method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
					&& method.Identifier.Text.Equals("Main", StringComparison.OrdinalIgnoreCase))
				{
					HasMainMethod = true;
				}

				// Look for an AppDelegate implementation of some sort
				if (syntaxNode is SimpleBaseTypeSyntax baseTypeSyntax)
				{
					var baseId = GlobalizeNs(baseTypeSyntax.ToFullString().Trim());

					if (baseId.Equals(GlobalizeNs("Microsoft.Maui.MauiUIApplicationDelegate")))
					{
						HasAppDelegateSubclass = true;

						// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
						// or if it's something else
						var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
							n is ClassDeclarationSyntax cp
							&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

						if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
						{
							PartialAppDelegateSubclass = classDeclParent.GetTypeInfoParts();
							
							// Check if the partial class overrides the CreateApp or not to decide if we emit it
							PartialAppDelegateOverridesCreate = HasCreateMauiAppOverride(context, classDeclParent);
						}
					}
					else if (baseId.Equals(GlobalizeNs("UIKit.UIApplicationDelegate")))
					{
						HasAppDelegateSubclass = true;
					}
				}
			}
		}

		class AndroidMauiBuilderReceiver
		{
			public bool HasApplicationSubclass { get; set; } = false;
			public TypeInfoParts? PartialApplicationSubclass { get; set; } = null;
			public bool HasPartialApplicationCreateOverride { get; set; } = false;

			public bool HasMainLauncher { get; set; } = false;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
			{
				if (!HasMainLauncher && syntaxNode is AttributeSyntax attrSyntax)
				{
					var attrFullName = attrSyntax.Name.ToFullString();
					if (attrSyntax.Name.ToFullString() == "global::Android.App.Activity")
					{
						HasMainLauncher = attrSyntax.ArgumentList != null
								&& attrSyntax.ArgumentList.ChildNodes().Any(cn => cn is AttributeArgumentSyntax aas
									&& aas.NameEquals?.Name?.Identifier.ValueText == "MainLauncher"
									&& aas.Expression is ExpressionSyntax exs
									&& exs.Kind() == SyntaxKind.TrueLiteralExpression);
					}
				}

				// Look for an AppDelegate implementation of some sort
				if (syntaxNode is SimpleBaseTypeSyntax baseTypeSyntax)
				{
					var baseId = GlobalizeNs(baseTypeSyntax.ToFullString().Trim());

					if (baseId.Equals("global::Microsoft.Maui.MauiApplication") 
						|| baseId.Equals("global::Android.App.Application"))
					{
						HasApplicationSubclass = true;

						if (baseId.Equals("global::Microsoft.Maui.MauiApplication"))
						{
							// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
							// or if it's something else
							var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
								n is ClassDeclarationSyntax cp
								&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

							if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
							{
								PartialApplicationSubclass = classDeclParent.GetTypeInfoParts();

								// Now check if they override CreateMauiApp() to see if we need to emit it or not
								HasPartialApplicationCreateOverride = HasCreateMauiAppOverride(context, classDeclParent);
							}
						}
					}
				}
			}
		}

		class AppStartupSyntaxReceiver : ISyntaxContextReceiver
		{
			public readonly MaciOSMauiAppBuilderReceiver MaciOS = new ();
			public readonly AndroidMauiBuilderReceiver Android = new ();

			public string? MauiAppBuilderMethod { get; set; } = null;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
			{
				var syntaxNode = context.Node;

				// Look for the static method entry point that returns a MauiApp
				if (syntaxNode is MethodDeclarationSyntax mds)
				{
					if (mds.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
					{
						var returnTypeFullNamme = context.GetReturnType(mds);

						if (returnTypeFullNamme == "Microsoft.Maui.MauiApp")
						{
							if (mds.Parent is ClassDeclarationSyntax cds)
							{
								var fullClassName = cds.GetFullName();
								var fullMethodName = mds.GetMethodName();

								MauiAppBuilderMethod = $"global::{fullClassName}.{fullMethodName}";
							}
						}
					}
				}

				Android.OnVisitSyntaxNode(context, syntaxNode);
				MaciOS.OnVisitSyntaxNode(context, syntaxNode);
			}
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!context.IsAppHead() || !context.IsMaui())
				return;

			// Allow opting out
			if (!context.GetMSBuildProperty("EnableMauiAppBuilderSourceGen", "true").Equals("true", StringComparison.OrdinalIgnoreCase))
				return;

			var isIos = context.IsiOS();
			var isAndroid = context.IsAndroid();
			var isMacCatalyst = context.IsMacCatalyst();
			var isWindows = context.IsWindows();

			if (!isIos && !isAndroid && !isMacCatalyst && !isWindows)
				return;

			// TODO: Maybe this should result in an error?
			if (context.SyntaxContextReceiver is AppStartupSyntaxReceiver syntaxReceiver && syntaxReceiver != null)
			{
				var namespaceName = context.Compilation.GlobalNamespace.Name;

				if (string.IsNullOrEmpty(namespaceName))
					namespaceName = context.Compilation.AssemblyName ?? "GeneratedMauiApp";

				if (syntaxReceiver.MauiAppBuilderMethod == null || string.IsNullOrEmpty(syntaxReceiver.MauiAppBuilderMethod))
					return;

				if (isIos || isMacCatalyst)
				{
					var wrapDefine = isIos ? "IOS" : "MACCATALYST";

					// Prefix with global:: when we generate code to be safe
					var appDelegateClassName = syntaxReceiver.MaciOS?.PartialAppDelegateSubclass?.GetTypeName()
						?? "AppDelegate";


					if (appDelegateClassName.Contains("."))
					{
						context.ReportDiagnostic(Diagnostic.Create("MAUI1050", "Compiler", "AppDelegate cannot be a nested type.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
						return;
					}

					// Create an app delegate
					// Only if there's either no delegate class at all already
					// Or if there is one, but it doesn't have the CreateApp override method in it
					// In either case we'd need to generate the partial subclass and the CreateApp override
					if (!syntaxReceiver.MaciOS!.HasAppDelegateSubclass ||
						(syntaxReceiver.MaciOS!.PartialAppDelegateSubclass != null && !syntaxReceiver.MaciOS!.PartialAppDelegateOverridesCreate))
					{
						if (syntaxReceiver.MaciOS.PartialAppDelegateSubclass != null)
							namespaceName = syntaxReceiver.MaciOS.PartialAppDelegateSubclass.GetNamespace();

						context.AddSource("Maui_Generated_MauiGeneratedAppDelegate.cs",
							GenerateMaciOSAppDelegate(wrapDefine, namespaceName, appDelegateClassName, syntaxReceiver.MauiAppBuilderMethod));
					}
					else
						context.ReportDiagnostic(Diagnostic.Create("MAUI1020", "Compiler", "UIApplicationDelegate implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));

					// Create a main method
					if (!syntaxReceiver.MaciOS.HasMainMethod)
					{
						context.AddSource("Maui_Generated_MauiGeneratedMain.cs",
							GenerateMaciOSMain(wrapDefine, namespaceName, "GeneratedProgramWithMain", appDelegateClassName));
					}
					else
						context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Main method implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
				}
				else if (isAndroid)
				{
					var appName = context.GetMSBuildProperty("ApplicationTitle") ?? "App1";

					if (!syntaxReceiver.Android.HasMainLauncher)
					{
						context.AddSource("Maui_Generated_MauiGeneratedAndroidActivity.cs",
						GenerateAndroidMainActivity(appName, "MainActivity", namespaceName));
					}
					else
					{
						context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Activity with MainLauncher=true already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
					}

					// Prefix with global:: when we generate code to be safe
					var applicationClassName = syntaxReceiver.Android.PartialApplicationSubclass?.GetTypeName()
						?? $"GeneratedMainApplication";

					if (applicationClassName.Contains("."))
					{
						context.ReportDiagnostic(Diagnostic.Create("MAUI1050", "Compiler", "Application cannot be a nested type.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
						return;
					}

					if (syntaxReceiver is not null)
					{
						// Create an application class
						// Only if there's either no application subclass at all already
						// Or if there is one, but it doesn't have the CreateApp override method in it
						// In either case we'd need to generate the partial subclass and the CreateApp override
						if (!syntaxReceiver.Android.HasApplicationSubclass ||
							(syntaxReceiver.Android.PartialApplicationSubclass != null && !syntaxReceiver.Android.HasPartialApplicationCreateOverride))
						{
							if (syntaxReceiver.Android.PartialApplicationSubclass != null)
								namespaceName = syntaxReceiver.Android.PartialApplicationSubclass.GetNamespace();

							context.AddSource("Maui_Generated_MauiGeneratedAndroidApplication.cs",
								GenerateAndroidApplication(namespaceName, applicationClassName, syntaxReceiver.MauiAppBuilderMethod));
						}
						else
							context.ReportDiagnostic(Diagnostic.Create("MAUI1020", "Compiler", "UIApplicationDelegate implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
					}
				}
			}
		}

		static string GlobalizeNs(string fullyQualifiedType)
		{
			if (!fullyQualifiedType.StartsWith("global::"))
				return "global::" + fullyQualifiedType;

			return fullyQualifiedType;
		}

		string GenerateAndroidMainActivity(string appName, string mainActivityClassName, string namespaceName)
			=> @"
#if ANDROID
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.SourceGen.Core"", ""1.0.0.0"")]
	[global::Android.App.Activity(Label = """ + appName + @""", MainLauncher = true)]
	public partial class " + mainActivityClassName + @" : global::Microsoft.Maui.MauiAppCompatActivity
	{
	}
}
#endif
";

		string GenerateAndroidApplication(string namespaceName, string applicationClassName, string createAppMethod)
			=> @"
#if ANDROID
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.SourceGen.Core"", ""1.0.0.0"")]
	[global::Android.App.Application]
	public partial class " + applicationClassName + @" : global::Microsoft.Maui.MauiApplication
	{
		protected override global::Microsoft.Maui.MauiApp CreateMauiApp()
			=> " + createAppMethod + @"();
	}
}
#endif
";
		string GenerateMaciOSMain(string wrapDefine, string namespaceName, string applicationClassName, string appDelegateClassName)
			=> @"
#if " + wrapDefine + @"
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.SourceGen.Core"", ""1.0.0.0"")]
    public partial class " + applicationClassName + @"
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            global::UIKit.UIApplication.Main(args, null, typeof(" + appDelegateClassName + @"));
        }
    }
}
#endif
";

		string GenerateMaciOSAppDelegate(string wrapDefine, string namespaceName, string appDelegateClassName, string createAppMethod)
			=> @"
#if " + wrapDefine + @"
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.SourceGen.Core"", ""1.0.0.0"")]
	[global::Foundation.Register(nameof(" + appDelegateClassName + @"))]
	public partial class " + appDelegateClassName + @" : global::Microsoft.Maui.MauiUIApplicationDelegate
	{
		protected override global::Microsoft.Maui.MauiApp CreateMauiApp()
			=> " + createAppMethod + @"();
	}
}
#endif
";
	}
}
