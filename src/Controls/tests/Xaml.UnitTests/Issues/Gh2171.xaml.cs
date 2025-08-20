using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh2171 : ContentPage
{
	public Gh2171() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void ParsingNestedMarkups([Values] XamlInflator inflator)
		{
			var layout = new Gh2171(inflator);
			var markup = layout.BindingContext as Gh2171Extension;
			Assert.That(markup, Is.Not.Null);
			Assert.That(markup.Foo, Is.EqualTo("foo"));
			Assert.That(markup.Bar, Is.EqualTo("bar"));
			Assert.That((markup.Binding as Binding).Path, Is.EqualTo("Text"));
		}
	}
}

[AcceptEmptyServiceProvider]
public class Gh2171Extension : IMarkupExtension
{
	public string Foo { get; set; }
	public string Bar { get; set; }
	public BindingBase Binding { get; set; }
	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => this;
}
