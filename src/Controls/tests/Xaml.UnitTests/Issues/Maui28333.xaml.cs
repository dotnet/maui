using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui28333 : VerticalStackLayout
{
	public Maui28333() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void CompiledBindingPreservesDottedKey(XamlInflator inflator)
		{
			var view = new Maui28333(inflator) { BindingContext = new Maui28333ViewModel() };
			Assert.IsType<TypedBinding<Maui28333ViewModel, string>>(view.dottedKey.GetContext(Label.TextProperty).Bindings.GetValue());
			Assert.IsType<TypedBinding<Maui28333ViewModel, int>>(view.dottedKeyLength.GetContext(Label.TextProperty).Bindings.GetValue());
			Assert.Equal("hello", view.dottedKey.Text);
			Assert.Equal("5", view.dottedKeyLength.Text);
		}

		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void CompiledBindingPreservesPropertyAfterLiteralBracket(XamlInflator inflator)
		{
			var view = new Maui28333(inflator) { BindingContext = new Maui28333ViewModel() };
			Assert.Equal("4", view.openBracketKeyLength.Text);
			// XamlC takes the last closing bracket in a part; SourceGen takes the first.
			Assert.Equal(inflator == XamlInflator.XamlC ? "4" : "1", view.closeBracketKeyLength.Text);
		}
	}
}

class Maui28333ViewModel
{
	public Dictionary<string, string> Items { get; } = new()
	{
		["he.llo"] = "hello",
		["a[b"] = "word",
		["a]"] = "four",
		["a"] = "x",
		["a].b"] = "dotted",
		["a]b[c.d"] = "literal",
		["a].b]c[d.e"] = "chained",
	};

	public Dictionary<string, Maui28333ViewModel> Nested => new() { ["a].b"] = this };
}
