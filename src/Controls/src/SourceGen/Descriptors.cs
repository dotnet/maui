using System;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1032 // The diagnostic message should not contain any line return character nor any leading or trailing whitespaces and should either be a single sentence without a trailing period or a multi-sentences with a trailing period

namespace Microsoft.Maui.Controls.SourceGen
{
	public static class Descriptors
	{
		//XamlParsing, general
		public static DiagnosticDescriptor XamlParserError = new DiagnosticDescriptor(
			id: "MAUIG1001",
			title: new LocalizableResourceString(nameof(MauiGResources.XamlParsingFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.XamlParsingErrorMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor AmbiguousType = new DiagnosticDescriptor(
			id: "MAUIG1002",
			title: new LocalizableResourceString(nameof(MauiGResources.AmbiguousTypeTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.AmbiguousTypeMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ExpressionNotClosed = new DiagnosticDescriptor(
			id: "MAUIG1003",
			title: new LocalizableResourceString(nameof(MauiGResources.XamlParsingFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ExpressionNotClosed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		//conversion
		public static DiagnosticDescriptor ConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor RectConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.RectConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor PointConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.PointConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ThicknessConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ThicknessConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor CornerRadiusConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.CornerRadiusConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor EasingConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.EasingConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor FlexBasisConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.FlexBasisConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor FlowDirectionConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.FlowDirectionConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor GridLengthConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.GridLengthConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ListStringConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ListStringConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor StrokeShapeConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.StrokeShapeConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor ColumnDefinitionCollectionConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.ColumnDefinitionCollectionConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor RowDefinitionCollectionConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.RowDefinitionCollectionConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor LayoutOptionsConversionFailed = new DiagnosticDescriptor(
			id: "MAUIG1010",
			title: new LocalizableResourceString(nameof(MauiGResources.ConversionFailedTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.LayoutOptionsConversionFailed), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		//Symbol resolution
		public static DiagnosticDescriptor TypeResolution = new DiagnosticDescriptor(
			id: "MAUIX2000",
			title: new LocalizableResourceString(nameof(MauiGResources.SymbolResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.TypeResolutionMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor DuplicateTypeError = new DiagnosticDescriptor(
			id: "MAUIX2001",
			title: new LocalizableResourceString(nameof(MauiGResources.SymbolResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.DuplicateTypeError), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor MemberResolution = new DiagnosticDescriptor(
			id: "MAUIX2002",
			title: new LocalizableResourceString(nameof(MauiGResources.SymbolResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.MemberResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor MethodResolution = new DiagnosticDescriptor(
			id: "MAUIX2003",
			title: new LocalizableResourceString(nameof(MauiGResources.SymbolResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.MethodResolution), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

		public static DiagnosticDescriptor DuplicateKeyInRD = new DiagnosticDescriptor(
			id: "MAUIX2004",
			title: new LocalizableResourceString(nameof(MauiGResources.DuplicateKeyInRD), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.DuplicateKeyInRD), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);

			public static DiagnosticDescriptor RequiredProperty = new DiagnosticDescriptor(
			id: "MAUIX2005",
			title: new LocalizableResourceString(nameof(MauiGResources.RequiredPropertyTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.RequiredPropertyMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlInflation",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		// public static BuildExceptionCode TypeResolution = new BuildExceptionCode("XC", 0000, nameof(TypeResolution), "");
		// public static BuildExceptionCode PropertyResolution = new BuildExceptionCode("XC", 0001, nameof(PropertyResolution), "");
		// public static BuildExceptionCode MissingEventHandler = new BuildExceptionCode("XC", 0002, nameof(MissingEventHandler), "");
		// public static BuildExceptionCode PropertyMissing = new BuildExceptionCode("XC", 0003, nameof(PropertyMissing), "");
		// public static BuildExceptionCode ConstructorDefaultMissing = new BuildExceptionCode("XC", 0004, nameof(ConstructorDefaultMissing), "");
		// public static BuildExceptionCode ConstructorXArgsMissing = new BuildExceptionCode("XC", 0005, nameof(ConstructorXArgsMissing), "");
		// public static BuildExceptionCode MethodStaticMissing = new BuildExceptionCode("XC", 0006, nameof(MethodStaticMissing), "");
		// public static BuildExceptionCode EnumValueMissing = new BuildExceptionCode("XC", 0007, nameof(EnumValueMissing), "");
		// public static BuildExceptionCode AdderMissing = new BuildExceptionCode("XC", 0008, nameof(AdderMissing), "");
		// public static BuildExceptionCode MemberResolution = new BuildExceptionCode("XC", 0009, nameof(MemberResolution), "");

		// //BP,BO
		// public static BuildExceptionCode BPName = new BuildExceptionCode("XC", 0020, nameof(BPName), "");
		// public static BuildExceptionCode BPMissingGetter = new BuildExceptionCode("XC", 0021, nameof(BPMissingGetter), "");
		// public static BuildExceptionCode BindingWithoutDataType = new BuildExceptionCode("XC", 0022, nameof(BindingWithoutDataType), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings"); //warning
		// public static BuildExceptionCode BindingWithNullDataType = new BuildExceptionCode("XC", 0023, nameof(BindingWithNullDataType), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings"); //warning
		// public static BuildExceptionCode BindingWithXDataTypeFromOuterScope = new BuildExceptionCode("XC", 0024, nameof(BindingWithXDataTypeFromOuterScope), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings");
		// public static BuildExceptionCode BindingWithSourceCompilationSkipped = new BuildExceptionCode("XC", 0025, nameof(BindingWithSourceCompilationSkipped), "https://learn.microsoft.com/dotnet/maui/fundamentals/data-binding/compiled-bindings"); //warning

		// //Bindings, conversions
		// public static BuildExceptionCode Conversion = new BuildExceptionCode("XC", 0040, nameof(Conversion), "");
		// public static BuildExceptionCode BindingIndexerNotClosed = new BuildExceptionCode("XC", 0041, nameof(BindingIndexerNotClosed), "");
		// public static BuildExceptionCode BindingIndexerEmpty = new BuildExceptionCode("XC", 0042, nameof(BindingIndexerEmpty), "");
		// public static BuildExceptionCode BindingIndexerTypeUnsupported = new BuildExceptionCode("XC", 0043, nameof(BindingIndexerTypeUnsupported), "");
		// public static BuildExceptionCode BindingIndexerParse = new BuildExceptionCode("XC", 0044, nameof(BindingIndexerParse), "");
		// public static BuildExceptionCode BindingPropertyNotFound = new BuildExceptionCode("XC", 0045, nameof(BindingPropertyNotFound), "");

		// //XAML issues
		// public static BuildExceptionCode MarkupNotClosed = new BuildExceptionCode("XC", 0060, nameof(MarkupNotClosed), "");
		// public static BuildExceptionCode MarkupParsingFailed = new BuildExceptionCode("XC", 0061, nameof(MarkupParsingFailed), "");
		// public static BuildExceptionCode XmlnsUndeclared = new BuildExceptionCode("XC", 0062, nameof(XmlnsUndeclared), "");
		// public static BuildExceptionCode SByteEnums = new BuildExceptionCode("XC", 0063, nameof(SByteEnums), "");
		// public static BuildExceptionCode NamescopeDuplicate = new BuildExceptionCode("XC", 0064, nameof(NamescopeDuplicate), "");
		// public static BuildExceptionCode ContentPropertyAttributeMissing = new BuildExceptionCode("XC", 0065, nameof(ContentPropertyAttributeMissing), "");
		// public static BuildExceptionCode InvalidXaml = new BuildExceptionCode("XC", 0066, nameof(InvalidXaml), "");


		// //Extensions
		// public static BuildExceptionCode XStaticSyntax = new BuildExceptionCode("XC", 0100, nameof(XStaticSyntax), "");
		// public static BuildExceptionCode XStaticResolution = new BuildExceptionCode("XC", 0101, nameof(XStaticResolution), "");
		// public static BuildExceptionCode XDataTypeSyntax = new BuildExceptionCode("XC", 0102, nameof(XDataTypeSyntax), "");
		// public static BuildExceptionCode UnattributedMarkupType = new BuildExceptionCode("XC", 0103, nameof(UnattributedMarkupType), ""); //warning

		// //Style, StyleSheets, Resources
		// public static BuildExceptionCode StyleSheetSourceOrContent = new BuildExceptionCode("XC", 0120, nameof(StyleSheetSourceOrContent), "");
		// public static BuildExceptionCode StyleSheetNoSourceOrContent = new BuildExceptionCode("XC", 0121, nameof(StyleSheetNoSourceOrContent), "");
		// public static BuildExceptionCode StyleSheetStyleNotALiteral = new BuildExceptionCode("XC", 0122, nameof(StyleSheetStyleNotALiteral), "");
		// public static BuildExceptionCode StyleSheetSourceNotALiteral = new BuildExceptionCode("XC", 0123, nameof(StyleSheetSourceNotALiteral), "");
		// public static BuildExceptionCode ResourceMissing = new BuildExceptionCode("XC", 0124, nameof(ResourceMissing), "");
		// public static BuildExceptionCode ResourceDictDuplicateKey = new BuildExceptionCode("XC", 0125, nameof(ResourceDictDuplicateKey), "");
		// public static BuildExceptionCode ResourceDictMissingKey = new BuildExceptionCode("XC", 0126, nameof(ResourceDictMissingKey), "");
		// public static BuildExceptionCode XKeyNotLiteral = new BuildExceptionCode("XC", 0127, nameof(XKeyNotLiteral), "");
		// public static BuildExceptionCode StaticResourceSyntax = new BuildExceptionCode("XC", 0128, nameof(StaticResourceSyntax), "");

		// //CSC equivalents
		// public static BuildExceptionCode ObsoleteProperty = new BuildExceptionCode("XC", 0618, nameof(ObsoleteProperty), ""); //warning

	}
}

