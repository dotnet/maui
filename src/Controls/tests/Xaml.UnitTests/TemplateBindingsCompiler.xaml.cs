using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TemplateBindingsCompiler : ContentPage
{
	public TemplateBindingsCompiler() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void Test([Values] XamlInflator inflator)
		{
			var page = new TemplateBindingsCompiler(inflator);
			var label = (Label)page.ContentView.GetTemplateChild("CardTitleLabel");
			Assert.AreEqual("The title", label?.Text);

			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
			{
				var binding = label.GetContext(Label.TextProperty).Bindings.GetValue();
				Assert.That(binding, Is.TypeOf<TypedBinding<TemplateBindingCompilerTestCardView, string>>());
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