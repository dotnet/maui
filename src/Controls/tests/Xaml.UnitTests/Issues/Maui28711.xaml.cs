// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui28711 : ContentPage
{
	public Maui28711() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void XNameOnResourceShouldNotCrash([Values] XamlInflator inflator)
		{
			// This test reproduces issue #28711
			// When using x:Name on a SolidColorBrush in Resources, a NullReferenceException
			// was thrown because GetHashCode was called before Color was set.
			var page = new Maui28711(inflator);

			Assert.That(page, Is.Not.Null);
			Assert.That(page.namedBrush, Is.Not.Null);
			Assert.That(page.namedBrush.Color, Is.EqualTo(Colors.Red));
		}
	}
}
