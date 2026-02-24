// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty(nameof(Text))]
[AcceptEmptyServiceProvider]
public class Gh9212MarkupExtension : IMarkupExtension
{
	public string Text { get; set; }
	public object ProvideValue(IServiceProvider serviceProvider) => Text;
}

public partial class Gh9212 : ContentPage
{
	public Gh9212() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SingleQuoteAndTrailingSpaceInMarkupValue(XamlInflator inflator)
		{
			var layout = new Gh9212(inflator);
			Assert.Equal("Foo, Bar", layout.label.Text);
		}
	}
}
