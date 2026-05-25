// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui28711 : ContentPage
{
	public Maui28711() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void XNameOnResourceShouldNotCrash(XamlInflator inflator)
		{
			// This test reproduces issue #28711
			// When using x:Name on a SolidColorBrush in Resources, a NullReferenceException
			// was thrown because GetHashCode was called before Color was set.
			var page = new Maui28711(inflator);

			Assert.NotNull(page);
			Assert.NotNull(page.namedBrush);
			Assert.Equal(Colors.Red, page.namedBrush.Color);
		}
	}
}
