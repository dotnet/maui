﻿using System;
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

		public static DiagnosticDescriptor AmbiguousType = new DiagnosticDescriptor(
			id: "MAUIG1002",
			title: new LocalizableResourceString(nameof(MauiGResources.AmbiguousTypeTitle), MauiGResources.ResourceManager, typeof(MauiGResources)),
			messageFormat: new LocalizableResourceString(nameof(MauiGResources.AmbiguousTypeMessage), MauiGResources.ResourceManager, typeof(MauiGResources)),
			category: "XamlParsing",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true);
	}
}

