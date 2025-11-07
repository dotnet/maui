// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh12763 : ContentPage
{
	public Gh12763() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void QuotesInStringFormat(XamlInflator inflator)
		{
			var layout = new Gh12763(inflator);
			Assert.Equal("\"Foo\"", layout.label.Text);
		}
	}
}
