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

		class AppStartupSyntaxReceiver : ISyntaxContextReceiver
		{
			public bool HasiOSMainMethod { get; set; } = false;
			public bool HasiOSAppDelegateSubclass { get; set; } = false;
			public string? iOSPartialAppDelegateSubclassType { get; set; } = null;
			public string? iOSPartialAppDelegateSubclassNs { get; set; } = null;
			public bool HasiOSPartialAppDelegateCreateOverride { get; set; } = false;

			public bool HasAndroidApplicationSubclass { get; set; } = false;
			public string? AndroidPartialApplicationSubclassType { get; set; } = null;
			public bool HasAndroidPartialApplicationCreateOverride { get; set; } = false;

			public bool HasAndroidMainLauncher { get; set; } = false;

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

				// Look for a Main method
				if (!HasiOSMainMethod
					&& syntaxNode is MethodDeclarationSyntax method
					&& method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
					&& method.Identifier.Text.Equals("Main", StringComparison.OrdinalIgnoreCase))
				{
					HasiOSMainMethod = true;
				}

				if (!HasAndroidMainLauncher && syntaxNode is AttributeSyntax attrSyntax && attrSyntax.Name.ToFullString() == "Activity")
				{
					HasAndroidMainLauncher = attrSyntax.ArgumentList != null
							&& attrSyntax.ArgumentList.ChildNodes().Any(cn => cn is AttributeArgumentSyntax aas
								&& aas.NameEquals?.Name?.Identifier.ValueText == "MainLauncher"
								&& aas.Expression is ExpressionSyntax exs
								&& exs.Kind() == SyntaxKind.TrueLiteralExpression);
				}

				// Look for an AppDelegate implementation of some sort
				if (syntaxNode is SimpleBaseTypeSyntax baseTypeSyntax)
				{
					var baseId = GlobalizeNs(baseTypeSyntax.ToFullString().Trim());

					if (baseId.Equals(GlobalizeNs("Microsoft.Maui.MauiUIApplicationDelegate")))
					{
						HasiOSAppDelegateSubclass = true;

						// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
						// or if it's something else
						var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
							n is ClassDeclarationSyntax cp
							&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

						if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
						{
							iOSPartialAppDelegateSubclassType = GlobalizeNs(classDeclParent.GetFullName());
							iOSPartialAppDelegateSubclassNs = classDeclParent.GetNamespace();
							
							// Now check if they override CreateMauiApp() to see if we need to emit it or not
							HasiOSPartialAppDelegateCreateOverride = HasCreateMauiAppOverride(context, classDeclParent);
						}
					}
					else if (baseId.Equals(GlobalizeNs("UIKit.UIApplicationDelegate")))
					{
						HasiOSAppDelegateSubclass = true;
					}

					if (baseId.Equals(GlobalizeNs("Microsoft.Maui.MauiApplication")) || baseId.Equals(GlobalizeNs("Android.App.Application")))
					{
						HasAndroidApplicationSubclass = true;

						if (baseId.Equals(GlobalizeNs("Microsoft.Maui.MauiApplication")))
						{
							// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
							// or if it's something else
							var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
								n is ClassDeclarationSyntax cp
								&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

							if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
							{
								AndroidPartialApplicationSubclassType = GlobalizeNs(classDeclParent.GetFullName());

								// Now check if they override CreateMauiApp() to see if we need to emit it or not
								HasAndroidPartialApplicationCreateOverride = HasCreateMauiAppOverride(context, classDeclParent);
							}
						}
					}
				}

				
			}

			static bool HasCreateMauiAppOverride(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
			{
				// Now check if they override CreateMauiApp() to see if we need to emit it or not
				return classDeclarationSyntax.Members.Any(m =>
					m is MethodDeclarationSyntax omds
					&& omds.IsKind(CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)
					&& (context.GetReturnType(omds)?.Equals("Microsoft.Maui.MauiApp") ?? false)
					&& omds.Identifier.Text == "CreateMauiApp");
			}
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!context.IsAppHead() || !context.IsMaui())
				return;

			var isIos = context.IsiOS();
			var isAndroid = context.IsAndroid();
			var isMacCatalyst = context.IsMacCatalyst();
			var isWindows = context.IsWindows();

			if (!isIos && !isAndroid && !isMacCatalyst && !isWindows)
				return;

			// TODO: Maybe this should result in an error?
			if (context.SyntaxContextReceiver is AppStartupSyntaxReceiver syntaxReceiver && syntaxReceiver is not null)
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
					var appDelegateClassName = GlobalizeNs(syntaxReceiver.iOSPartialAppDelegateSubclassType
						?? $"{namespaceName}.AppDelegate");

					// Create an app delegate
					// We check to see if there's already an app delegate
					// If there is, if it's partial and the same name as the one we generate, that's fine too
					if (!syntaxReceiver.HasiOSAppDelegateSubclass ||
						(syntaxReceiver.iOSPartialAppDelegateSubclassType != null && !syntaxReceiver.HasiOSPartialAppDelegateCreateOverride))
					{
						if (!string.IsNullOrEmpty(syntaxReceiver.iOSPartialAppDelegateSubclassNs))
							namespaceName = syntaxReceiver.iOSPartialAppDelegateSubclassNs!;

						context.AddSource("Maui_Generated_MauiGeneratedAppDelegate.cs",
							GenerateiOSAppDelegate(wrapDefine, namespaceName, appDelegateClassName, syntaxReceiver.MauiAppBuilderMethod));
					}
					else
						context.ReportDiagnostic(Diagnostic.Create("MAUI1020", "Compiler", "UIApplicationDelegate implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));

					// Create a main method
					if (!(syntaxReceiver?.HasiOSMainMethod ?? false))
					{
						context.AddSource("Maui_Generated_MauiGeneratedMain.cs",
							GenerateiOSMain(wrapDefine, namespaceName, appDelegateClassName));
					}
					else
						context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Main method implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
				}
				else if (isAndroid)
				{
					var appName = context.GetMSBuildProperty("ApplicationTitle") ?? "App1";


					if (!(syntaxReceiver?.HasAndroidMainLauncher ?? false))
					{
						context.AddSource("Maui_Generated_MauiGeneratedAndroidActivity.cs",
						GenerateAndroidMainActivity(appName, namespaceName));
					}
					else
					{
						context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Activity with MainLauncher=true already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
					}

					// Prefix with global:: when we generate code to be safe
					var applicationClassName = GlobalizeNs(syntaxReceiver?.AndroidPartialApplicationSubclassType
						?? $"{namespaceName}.MainApplication");

					if (syntaxReceiver is not null)
					{
						// If there's no existing application subclass
						// or if there is a subclass but it's partial, and it doesn't have the create maui app override yet
						if (!syntaxReceiver.HasAndroidApplicationSubclass ||
							(syntaxReceiver.AndroidPartialApplicationSubclassType != null && !syntaxReceiver.HasAndroidPartialApplicationCreateOverride))
						{
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

		string GenerateAndroidMainActivity(string appName, string namespaceName)
			=> @"
#if ANDROID
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.Controls.SourceGen"", ""1.0.0.0"")]
	[global::Android.App.Activity(Label = """ + appName + @""", MainLauncher = true)]
	public partial class MainActivity : global::Microsoft.Maui.MauiAppCompatActivity
	{
	}
}
#endif
";

		string GenerateAndroidApplication(string namespaceName, string applicationName, string createAppMethod)
			=> @"
#if ANDROID
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.Controls.SourceGen"", ""1.0.0.0"")]
	[global::Android.App.Application]
	public partial class " + applicationName + @" : global::Microsoft.Maui.MauiApplication
	{
		protected override global::Microsoft.Maui.MauiApp CreateMauiApp()
			=> " + createAppMethod + @"();
	}
}
#endif
";
		string GenerateiOSMain(string wrapDefine, string namespaceName, string appDelegateClassName)
			=> @"
#if " + wrapDefine + @"
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.Controls.SourceGen"", ""1.0.0.0"")]
    public partial class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(" + appDelegateClassName + @"));
        }
    }
}
#endif
";

		string GenerateiOSAppDelegate(string wrapDefine, string namespaceName, string appDelegateClassName, string createAppMethod)
			=> @"
#if " + wrapDefine + @"
using UIKit;
using Foundation;
namespace " + namespaceName + @"
{
	[global::System.CodeDom.Compiler.GeneratedCode(""Microsoft.Maui.Controls.SourceGen"", ""1.0.0.0"")]
	[Register(nameof(AppDelegate))]
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
