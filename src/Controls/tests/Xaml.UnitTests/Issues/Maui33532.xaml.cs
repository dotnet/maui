// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33532 : ContentPage
{
	public Maui33532() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void NaNValueInXamlGeneratesValidCode(XamlInflator inflator)
		{
			// This test reproduces issue #33532:
			// When a XAML file contains NaN as a value (e.g., Padding="NaN"),
			// the XAML Source Generator was generating invalid C# code using bare "NaN"
			// instead of "double.NaN", causing CS0103 compiler error.
			var page = new Maui33532(inflator);

			Assert.NotNull(page);
			Assert.NotNull(page.buttonNaN);
			Assert.NotNull(page.buttonNaNComma);
			Assert.NotNull(page.buttonNaNFull);

			// Verify NaN values were applied correctly (all sides should be NaN)
			Assert.True(double.IsNaN(page.buttonNaN.Padding.Left));
			Assert.True(double.IsNaN(page.buttonNaN.Padding.Top));
			Assert.True(double.IsNaN(page.buttonNaN.Padding.Right));
			Assert.True(double.IsNaN(page.buttonNaN.Padding.Bottom));

			// Verify 2-value NaN syntax (horizontal, vertical)
			Assert.True(double.IsNaN(page.buttonNaNComma.Padding.Left));
			Assert.True(double.IsNaN(page.buttonNaNComma.Padding.Top));

			// Verify 4-value NaN syntax
			Assert.True(double.IsNaN(page.buttonNaNFull.Padding.Left));
			Assert.True(double.IsNaN(page.buttonNaNFull.Padding.Top));
			Assert.True(double.IsNaN(page.buttonNaNFull.Padding.Right));
			Assert.True(double.IsNaN(page.buttonNaNFull.Padding.Bottom));
		}
	}
}
