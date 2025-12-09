using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TemplateBindingsCompiler : ContentPage
{
	public TemplateBindingsCompiler() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IClassFixture<DispatcherProviderFixture>
	{
		public Tests(DispatcherProviderFixture fixture) { }

		[Theory]
		[XamlInflatorData]
		internal void Test(XamlInflator inflator)
		{
			var page = new TemplateBindingsCompiler(inflator);
			var label = (Label)page.ContentView.GetTemplateChild("CardTitleLabel");
			Assert.Equal("The title", label?.Text);

			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
			{
				var binding = label.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.IsType<TypedBinding<TemplateBindingCompilerTestCardView, string>>(binding);
			}
		}
	}
}

public class TemplateBindingCompilerTestCardView : ContentView
{
	public static readonly BindableProperty CardTitleProperty =
		BindableProperty.Create(nameof(CardTitle), typeof(string), typeof(TemplateBindingCompilerTestCardView), string.Empty);

	public string CardTitle
	{
		get => (string)GetValue(CardTitleProperty);
		set => SetValue(CardTitleProperty, value);
	}
}