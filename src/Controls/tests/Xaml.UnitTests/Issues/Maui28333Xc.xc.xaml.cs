using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui28333Xc : VerticalStackLayout
{
	public Maui28333Xc() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Fact]
		public void XamlCKeepsDottedKeyContainingClosingBracket()
		{
			var view = new Maui28333Xc { BindingContext = new Maui28333ViewModel() };
			Assert.IsType<TypedBinding<Maui28333ViewModel, string>>(view.dottedKey.GetContext(Label.TextProperty).Bindings.GetValue());
			Assert.IsType<TypedBinding<Maui28333ViewModel, int>>(view.dottedKeyLength.GetContext(Label.TextProperty).Bindings.GetValue());
			Assert.IsType<TypedBinding<Maui28333ViewModel, int>>(view.chainedIndexer.GetContext(Label.TextProperty).Bindings.GetValue());
			Assert.Equal("dotted", view.dottedKey.Text);
			Assert.Equal("6", view.dottedKeyLength.Text);
			Assert.Equal("5", view.chainedIndexer.Text);
		}
	}
}
