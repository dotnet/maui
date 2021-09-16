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
		const string MsBuildSourceGenFlagPropertyName = "EnableMauiAppBuilderSourceGen";

		const string CreateMauiAppMethodName = "CreateMauiApp";
		const string MauiAppTypeFullyQualifiedName = "global::Microsoft.Maui.MauiApp";
		const string MauiUIApplicationDelegateFullyQualifiedName = "global::Microsoft.Maui.MauiUIApplicationDelegate";
		const string UIKitUIApplicationDelegateFullyQualifiedName = "global::UIKit.UIApplicationDelegate";
		const string AndroidAppActivityFullyQualifiedName = "global::Android.App.ActivityAttribute";
		const string MauiAndroidApplicationFullyQualifiedName = "global::Microsoft.Maui.MauiApplication";
		const string AndroidAppApplicationFullyQualifiedName = "global::Android.App.Application";
		const string AndroidMainLauncherAttributeName = "MainLauncher";
		const string AppDelegateDefaultClassName = "MauiAppDelegate";
		const string MainActivityDefaultClassName = "MauiMainActivity";
		const string ProgramDefaultClassName = "MauiProgram";
		const string MainMethodName = "Main";
		const string AndroidApplicationDefaultClassName = "MauiApplication";

		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new AppStartupSyntaxReceiver());
		}

		static bool HasCreateMauiAppOverride(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
		{
			foreach (var m in classDeclarationSyntax.Members)
			{
				if (m is MethodDeclarationSyntax omds
					&& omds.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword))
					&& omds.SyntaxTree != null)
				{
					var methodSymbol = omds.GetSymbol(context.SemanticModel.Compilation);
					if (methodSymbol?.Name?.Equals(CreateMauiAppMethodName) ?? false)
					{
						var returnTypeSymbol = context.SemanticModel.GetSymbolInfo(omds.ReturnType).Symbol;
						var rtsName = returnTypeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
						if (rtsName?.Equals(MauiAppTypeFullyQualifiedName) ?? false)
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

			public ClassDeclarationSyntax? PartialAppDelegateSubclass { get; set; } = null;

			public bool PartialAppDelegateOverridesCreate { get; set; } = false;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
			{
				var compilation = context.SemanticModel.Compilation;

				// Look for a Main method
				if (!HasMainMethod
					&& syntaxNode is MethodDeclarationSyntax method)
				{
					if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
					{
						var methodName = method.GetMethodName(context.SemanticModel.Compilation);
						if (methodName?.Equals(MainMethodName) ?? false)
						{
							HasMainMethod = true;
						}
					}
				}

				// Look for an AppDelegate implementation of some sort
				if (syntaxNode is BaseTypeSyntax baseTypeSyntax)
				{
					var baseTypeSymbol = context.SemanticModel.Compilation.GetSemanticModel(syntaxNode.SyntaxTree)
						.GetSymbolInfo(baseTypeSyntax.Type).Symbol;

					var baseTypeName = baseTypeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? String.Empty;

					if (baseTypeName.Equals(MauiUIApplicationDelegateFullyQualifiedName))
					{
						HasAppDelegateSubclass = true;

						// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
						// or if it's something else
						var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
							n is ClassDeclarationSyntax cp
							&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

						if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
						{
							PartialAppDelegateSubclass = classDeclParent;

							// Check if the partial class overrides the CreateApp or not to decide if we emit it
							PartialAppDelegateOverridesCreate = HasCreateMauiAppOverride(context, classDeclParent);
						}
					}
					else if (baseTypeName.Equals(UIKitUIApplicationDelegateFullyQualifiedName))
					{
						HasAppDelegateSubclass = true;
					}
				}
			}
		}

		class AndroidMauiBuilderReceiver
		{
			public bool HasApplicationSubclass { get; set; } = false;
			public ClassDeclarationSyntax? PartialApplicationSubclass { get; set; } = null;
			public bool HasPartialApplicationCreateOverride { get; set; } = false;

			public bool HasMainLauncher { get; set; } = false;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context, SyntaxNode syntaxNode)
			{
				var compilation = context.SemanticModel.Compilation;

				if (!HasMainLauncher && syntaxNode is AttributeSyntax attrSyntax)
				{
					//var attrFullName = attrSyntax.ToFullString(); // attrSyntax.GetSymbol(context.SemanticModel.Compilation)?.ToDisplayString();

					//if (attrFullName == AndroidAppActivityFullyQualifiedName)
					//{
					HasMainLauncher = attrSyntax.ArgumentList != null
							&& attrSyntax.ArgumentList.ChildNodes().Any(cn => cn is AttributeArgumentSyntax aas
								&& aas.NameEquals?.Name?.Identifier.ValueText == AndroidMainLauncherAttributeName
								&& aas.Expression is ExpressionSyntax exs
								&& exs.Kind() == SyntaxKind.TrueLiteralExpression);
					//}
				}

				// Look for an Application implementation of some sort
				if (syntaxNode is BaseTypeSyntax baseTypeSyntax)
				{
					var baseTypeSymbol = context.SemanticModel.Compilation.GetSemanticModel(syntaxNode.SyntaxTree)
						.GetSymbolInfo(baseTypeSyntax.Type).Symbol;

					if (baseTypeSymbol != null)
					{
						var baseTypeName = baseTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

						if (baseTypeName.Equals(MauiAndroidApplicationFullyQualifiedName)
							|| baseTypeName.Equals(AndroidAppApplicationFullyQualifiedName))
						{
							HasApplicationSubclass = true;

							if (baseTypeName.Equals(MauiAndroidApplicationFullyQualifiedName))
							{
								// Check if it's a partial declaration, if so, get the type name back and see if it's partial to us
								// or if it's something else
								var partialClassSyntax = baseTypeSyntax.Ancestors().FirstOrDefault(n =>
									n is ClassDeclarationSyntax cp
									&& cp.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));

								if (partialClassSyntax is ClassDeclarationSyntax classDeclParent)
								{
									PartialApplicationSubclass = classDeclParent;

									// Now check if they override CreateMauiApp() to see if we need to emit it or not
									HasPartialApplicationCreateOverride = HasCreateMauiAppOverride(context, classDeclParent);
								}
							}
						}
					}
				}
			}
		}

		class AppStartupSyntaxReceiver : ISyntaxContextReceiver
		{
			public readonly MaciOSMauiAppBuilderReceiver MaciOS = new();
			public readonly AndroidMauiBuilderReceiver Android = new();

			public IMethodSymbol? AppBuilderMethod { get; set; } = null;

			public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
			{
				var syntaxNode = context.Node;

				// Look for the static method entry point that returns a MauiApp
				if (AppBuilderMethod == null && syntaxNode is MethodDeclarationSyntax mds)
				{
					if (mds.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
					{
						var fullReturnTypeName = mds.GetReturnType(context.SemanticModel.Compilation)?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

						if (fullReturnTypeName == MauiAppTypeFullyQualifiedName
							&& mds.GetSymbol(context.SemanticModel.Compilation) is IMethodSymbol methodSymbol)
						{
							AppBuilderMethod = methodSymbol;
						}
					}
				}

				Android.OnVisitSyntaxNode(context, syntaxNode);
				MaciOS.OnVisitSyntaxNode(context, syntaxNode);
			}
		}

		public void Execute(GeneratorExecutionContext context)
		{
			// If it's not our receiver type, just return
			if (context.SyntaxContextReceiver is not AppStartupSyntaxReceiver syntaxReceiver)
				return;

			if (!context.IsMaui() || !context.IsAppHead())
				return;

			// Allow opting out
			if (!context.GetMSBuildProperty(MsBuildSourceGenFlagPropertyName, "true").Equals("true", StringComparison.OrdinalIgnoreCase))
				return;

			var isIos = context.IsiOS();
			var isAndroid = context.IsAndroid();
			var isMacCatalyst = context.IsMacCatalyst();
			var isWindows = context.IsWindows();

			if (!isIos && !isAndroid && !isMacCatalyst && !isWindows)
				return;

			if (syntaxReceiver.AppBuilderMethod == null)
				return;
			var builderMethodStr = syntaxReceiver.AppBuilderMethod.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
					+ "." + syntaxReceiver.AppBuilderMethod.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			if (string.IsNullOrEmpty(builderMethodStr))
				return;

			var namespaceName = context.Compilation.GlobalNamespace.Name;

			if (string.IsNullOrEmpty(namespaceName))
				namespaceName = context.Compilation.AssemblyName ?? "GeneratedMauiApp";

			if (isIos || isMacCatalyst)
			{
				var wrapDefine = isIos ? "IOS" : "MACCATALYST";

				var appDelegateClassName = syntaxReceiver.MaciOS?.PartialAppDelegateSubclass?.GetSymbol(context.Compilation)?.Name
					?? AppDelegateDefaultClassName;

				if (appDelegateClassName.Contains("."))
				{
					context.ReportDiagnostic(Diagnostic.Create("MAUI1050", "Compiler", "Application Delegate cannot be a nested type.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
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
						namespaceName = syntaxReceiver.MaciOS.PartialAppDelegateSubclass.GetSymbol(context.Compilation)?.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

					if (!string.IsNullOrEmpty(namespaceName))
						context.AddSource("Maui_Generated_iOS_AppDelegate.cs",
							GenerateMaciOSAppDelegate(wrapDefine, namespaceName!, appDelegateClassName, builderMethodStr!));
				}
				else
					context.ReportDiagnostic(Diagnostic.Create("MAUI1020", "Compiler", "UIApplicationDelegate implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));

				// Create a main method
				if (!syntaxReceiver.MaciOS.HasMainMethod)
				{
					var appDelegateClassSymbol = syntaxReceiver.MaciOS?.PartialAppDelegateSubclass?.GetSymbol(context.Compilation);

					var fqAppDelegateClassName = appDelegateClassSymbol?.ToDisplayString()
						?? AppDelegateDefaultClassName;

					context.AddSource("Maui_Generated_iOS_Program.cs",
						GenerateMaciOSMain(wrapDefine, namespaceName!, ProgramDefaultClassName, fqAppDelegateClassName));
				}
				else
					context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Main method implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
			}
			else if (isAndroid)
			{
				var appName = context.GetMSBuildProperty("ApplicationTitle") ?? "Application";

				if (!syntaxReceiver.Android.HasMainLauncher)
				{
					context.AddSource("Maui_Generated_Android_MainActivity.cs",
					GenerateAndroidMainActivity(appName, MainActivityDefaultClassName, namespaceName));
				}
				else
				{
					context.ReportDiagnostic(Diagnostic.Create("MAUI1021", "Compiler", "Activity with MainLauncher=true already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
				}

				var applicationClassName = syntaxReceiver.Android.PartialApplicationSubclass?.GetSymbol(context.Compilation)?.Name
					?? AndroidApplicationDefaultClassName;

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
							namespaceName = syntaxReceiver.Android.PartialApplicationSubclass.GetSymbol(context.Compilation)?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

						if (!string.IsNullOrEmpty(namespaceName))
							context.AddSource("Maui_Generated_Android_Application.cs",
								GenerateAndroidApplication(namespaceName!, applicationClassName, builderMethodStr!));
					}
					else
						context.ReportDiagnostic(Diagnostic.Create("MAUI1020", "Compiler", "UIApplicationDelegate implementation already exists, not generating one.", DiagnosticSeverity.Warning, DiagnosticSeverity.Warning, true, 3));
				}
			}
		}

		const string GeneratorAssemblyName = "Microsoft.Maui.SourceGen.Core";
		const string GeneratorAssemblyVersion = "1.0.0.0";

		string GenerateAndroidMainActivity(string appName, string mainActivityClassName, string namespaceName)
			=> @"
// <auto-generated/>
#if ANDROID
namespace " + namespaceName + @"
{
    [global::System.CodeDom.Compiler.GeneratedCode(""" + GeneratorAssemblyName + @""", """ + GeneratorAssemblyVersion + @""")]
    [global::Android.App.Activity(Label = """ + appName + @""", MainLauncher = true)]
    public partial class " + mainActivityClassName + @" : global::Microsoft.Maui.MauiAppCompatActivity
    {
    }
}
#endif
";

		string GenerateAndroidApplication(string namespaceName, string applicationClassName, string createAppMethod)
			=> @"
// <auto-generated/>
#if ANDROID
namespace " + namespaceName + @"
{
    [global::System.CodeDom.Compiler.GeneratedCode(""" + GeneratorAssemblyName + @""", """ + GeneratorAssemblyVersion + @""")]
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
// <auto-generated/>
#if " + wrapDefine + @"
namespace " + namespaceName + @"
{
    [global::System.CodeDom.Compiler.GeneratedCode(""" + GeneratorAssemblyName + @""", """ + GeneratorAssemblyVersion + @""")]
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
// <auto-generated/>
#if " + wrapDefine + @"
namespace " + namespaceName + @"
{
    [global::System.CodeDom.Compiler.GeneratedCode(""" + GeneratorAssemblyName + @""", """ + GeneratorAssemblyVersion + @""")]
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
