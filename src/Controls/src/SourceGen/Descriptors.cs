using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen
{
	public static class Descriptors
	{
		public static DiagnosticDescriptor XamlParserError = new DiagnosticDescriptor(
			id: "MAUIG1001",
			title: new LocalizableResourceString(nameof(MauiGResources.XamlParsingFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.XamlParsingErrorMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ExpressionNotClosed = new DiagnosticDescriptor(
			id: "MAUIG1002",
			title: new LocalizableResourceString(nameof(MauiGResources.XamlParsingFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ExpressionNotClosed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor RectConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1003",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.RectConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ColorConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1004",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ColorConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor PointConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1005",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.PointConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ThicknessConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1006",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ThicknessConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor CornerRadiusConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1007",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.CornerRadiusConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);
	}
}

