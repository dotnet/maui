// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33868 : ContentPage
{
	public Maui33868() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void PropertyElementWithAttributeShouldError(XamlInflator inflator)
		{
			// This test reproduces issue #33868:
			// Attributes on property elements (e.g., <Grid.RowDefinitions Foo="Bar">)
			// are invalid XAML. XSG should emit an error (not a warning).
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33868 : ContentPage
{
	public Maui33868() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33868));
				
				// Should have exactly one error diagnostic for the invalid attribute
				var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
				Assert.Single(errors);
				Assert.Equal("MAUIX2006", errors[0].Id);
				Assert.Contains("RowDefinitions", errors[0].GetMessage(), StringComparison.Ordinal);
			}
			else
			{
				// Runtime is more permissive (it just ignores the attribute)
				// XamlC is not generated for .rt.xaml files
				if (inflator == XamlInflator.XamlC)
				{
					Assert.Throws<NotSupportedException>(() => new Maui33868(inflator));
				}
				else
				{
					var page = new Maui33868(inflator);
					Assert.NotNull(page);
				}
			}
		}
	}
}
