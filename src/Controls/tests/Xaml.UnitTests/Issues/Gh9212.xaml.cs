// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void SingleQuoteAndTrailingSpaceInMarkupValue([Values] XamlInflator inflator)
		{
			var layout = new Gh9212(inflator);
			Assert.That(layout.label.Text, Is.EqualTo("Foo, Bar"));
		}
	}
}
