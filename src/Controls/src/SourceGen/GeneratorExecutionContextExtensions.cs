using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen
{
	internal static class GeneratorExecutionContextExtensions
	{
		public static string? GetMSBuildItemMetadata(
			this GeneratorExecutionContext context,
			AdditionalText additionalText,
			string name,
			string? defaultValue = default)
		{
			context.AnalyzerConfigOptions
				.GetOptions(additionalText)
				.TryGetValue($"build_metadata.AdditionalFiles.{name}", out var value);

			return value ?? defaultValue;
		}

		public static string GetMSBuildProperty(
		   this GeneratorExecutionContext context,
		   string name,
		   string defaultValue = "")
		{
			context
				.AnalyzerConfigOptions
				.GlobalOptions
				.TryGetValue($"build_property.{name}", out var value);
			return value ?? defaultValue;
		}

		public static string? GetCompilationContextTFM(this GeneratorExecutionContext context)
		{
			var attr = context.Compilation.Assembly.GetAttributes();

			foreach (var a in attr)
			{
				if (a.AttributeClass?.Name == "TargetFrameworkAttribute"
					|| a.AttributeClass?.Name == "TargetFramework")
				{
					if (a.ConstructorArguments.Length > 0)
						return a.ConstructorArguments[0].Value?.ToString();
				}
			}

			return null;
		}

		public static IEnumerable<AttributeData> FindAttributes(this GeneratorExecutionContext context, string attributeTypeName)
		{
			var attr = context.Compilation.Assembly.GetAttributes();

			foreach (var a in context.Compilation.Assembly.GetAttributes())
			{
				if (a.AttributeClass?.Name == attributeTypeName)
					yield return a;
			}

			foreach (var r in context.Compilation.References)
			{
				if (r.Properties.Kind == MetadataImageKind.Assembly)
				{
					var assembly = context.Compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol;

					if (assembly != null)
					{
						foreach (var a in assembly.GetAttributes())
							if (a.AttributeClass?.Name == attributeTypeName)
								yield return a;
					}
				}
			}
		}

		public static string? GetAssemblyMetadata(this GeneratorExecutionContext context, string key)
		{
			foreach (var a in context.FindAttributes("AssemblyMetadataAttribute"))
			{
				if (a.ConstructorArguments.Length == 2 && a.ConstructorArguments[0].Value?.ToString() == key)
					return a.ConstructorArguments[1].Value?.ToString();
			}

			return null;
		}

		public static bool IsiOS(this GeneratorExecutionContext context)
			=> (context.GetMSBuildProperty("TargetPlatformIdentifier")?.Equals("ios", StringComparison.OrdinalIgnoreCase) ?? false)
				|| (context.GetCompilationContextTFM()?.Contains("-ios") ?? false);

		public static bool IsMacCatalyst(this GeneratorExecutionContext context)
			=> (context.GetMSBuildProperty("TargetPlatformIdentifier")?.Equals("maccatalyst", StringComparison.OrdinalIgnoreCase) ?? false)
				|| (context.GetCompilationContextTFM()?.Contains("-maccatalyst") ?? false);

		public static bool IsAndroid(this GeneratorExecutionContext context)
			=> (context.GetMSBuildProperty("TargetPlatformIdentifier")?.Equals("android", StringComparison.OrdinalIgnoreCase) ?? false)
				|| (context.GetCompilationContextTFM()?.Contains("-android") ?? false);

		public static bool IsWindows(this GeneratorExecutionContext context)
			=> (context.GetMSBuildProperty("TargetPlatformIdentifier")?.Equals("windows", StringComparison.OrdinalIgnoreCase) ?? false)
				|| (context.GetCompilationContextTFM()?.Contains("-windows") ?? false);

		public static bool IsAppHead(this GeneratorExecutionContext context)
		{
			var outputType = context.GetMSBuildProperty("OutputType");
			
			return (outputType?.Equals("Exe", StringComparison.OrdinalIgnoreCase) ?? false)
				|| (outputType?.Equals("WinExe", StringComparison.OrdinalIgnoreCase) ?? false);
		}

		public static bool IsMaui(this GeneratorExecutionContext context)
			=> context.GetMSBuildProperty("UseMaui")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
	}
}
