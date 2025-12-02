// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32879 : ContentPage
{
	public Maui32879() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void StyleSetterWithAttachedPropertyContentSyntax([Values] XamlInflator inflator)
		{
			// This test reproduces issue #32879:
			// Style with setter on AbsoluteLayout.LayoutBounds and SourceGen fails
			// The bug was that using content property syntax (value between tags)
			// instead of attribute syntax generated invalid C# code like:
			// .Value = "10,10,20,20";
			// var setter90 = new global::Microsoft.Maui.Controls.Setter {...};
			var page = new Maui32879(inflator);

			Assert.That(page, Is.Not.Null);
			Assert.That(page.testImage, Is.Not.Null);

			// Verify the style was applied correctly
			var bounds = AbsoluteLayout.GetLayoutBounds(page.testImage);
			Assert.That(bounds, Is.EqualTo(new Rect(10, 10, 20, 20)));
		}
	}
}
