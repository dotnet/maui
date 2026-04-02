// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh7837VMBase //using a base class to test #2131
{
	public string this[int index] => "";
	public string this[string index] => "";
}

public class Gh7837VM : Gh7837VMBase
{
	public new string this[int index] => index == 42 ? "forty-two" : "dull number";
	public new string this[string index] => index.ToUpperInvariant();
}

public partial class Gh7837 : ContentPage
{
	public Gh7837() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void BindingWithMultipleIndexers(XamlInflator inflator)
		{
			var layout = new Gh7837(inflator);
			Assert.Equal("forty-two", layout.label0.Text);
			Assert.Equal("FOO", layout.label1.Text);
			Assert.Equal("forty-two", layout.label2.Text);
			Assert.Equal("FOO", layout.label3.Text);
		}
	}
}
