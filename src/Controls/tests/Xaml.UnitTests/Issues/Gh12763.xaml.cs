// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh12763 : ContentPage
{
	public Gh12763() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void QuotesInStringFormat([Values] XamlInflator inflator)
		{
			var layout = new Gh12763(inflator);
			Assert.That(layout.label.Text, Is.EqualTo("\"Foo\""));
		}
	}
}
