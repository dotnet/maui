// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33867 : ContentPage
{
	public Maui33867() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void InlineStyleSheetWithCDATA(XamlInflator inflator)
		{
			// This test reproduces issue #33867:
			// XSG doesn't correctly parse inline StyleSheet elements with CDATA sections.
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33867 : ContentPage
{
	public Maui33867() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33867));
				
				// Verify no compilation errors
				Assert.Empty(result.Diagnostics);
			}

			var page = new Maui33867(inflator);
			Assert.NotNull(page);
			Assert.NotNull(page.testLabel);
		}
	}
}
