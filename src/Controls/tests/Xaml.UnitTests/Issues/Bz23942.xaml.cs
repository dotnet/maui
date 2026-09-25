using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz23942Options : BindableObject
{
	public static readonly BindableProperty TextProperty =
		BindableProperty.Create(nameof(Text), typeof(string), typeof(Bz23942Options));

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}
}

public class Bz23942Label : Label
{
	public static readonly BindableProperty OptionsProperty =
		BindableProperty.Create(nameof(Options), typeof(Bz23942Options), typeof(Bz23942Label));

	public Bz23942Options Options
	{
		get => (Bz23942Options)GetValue(OptionsProperty);
		set => SetValue(OptionsProperty, value);
	}
}

public partial class Bz23942 : ContentPage
{
	public Bz23942() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void ResourceBindableObjectsInheritAndUpdateBindingContext(XamlInflator inflator)
		{
			var page = new Bz23942(inflator);
			var model = new Bz23942Options { Text = "initial binding works" };
			page.BindingContext = model;
			var options = page.TestLabel.Options;
			Assert.Same(page.Resources["Options"], options);
			Assert.Same(model, options.BindingContext);
			Assert.Equal("initial binding works", options.Text);
			model.Text = "success";
			Assert.Equal("success", options.Text);
			var replacement = new Bz23942Options { Text = "replacement" };
			page.BindingContext = replacement;
			Assert.Equal("replacement", options.Text);
			model.Text = "detached";
			Assert.Equal("replacement", options.Text);
			replacement.Text = "updated";
			Assert.Equal("updated", options.Text);
		}
	}
}
