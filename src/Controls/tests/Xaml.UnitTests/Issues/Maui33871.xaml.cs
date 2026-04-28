// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33871 : ContentPage
{
	public Maui33871() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ThicknessConverterHandlesNaN(XamlInflator inflator)
		{
			// This test reproduces issue #33871:
			// XSG fails to parse Padding="NaN" which creates a Thickness with all NaN values.
			// This is used by Button to indicate "use platform default padding".
			var page = new Maui33871(inflator);

			Assert.NotNull(page);
			Assert.NotNull(page.nanButton);

			// Verify the padding was parsed correctly as NaN
			Assert.True(page.nanButton.Padding.IsNaN);
		}
	}
}
