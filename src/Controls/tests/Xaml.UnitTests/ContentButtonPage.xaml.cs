using System.Windows.Input;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;
using AndroidContentButton = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ContentButton;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ContentButtonPage : ContentPage
{
	public ContentButtonPage() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IClassFixture<ApplicationFixture>
	{
		public Tests(ApplicationFixture fixture) { }

		[Theory]
		[XamlInflatorData]
		internal void ImplicitContentAndCompiledBindings(XamlInflator inflator)
		{
			var model = new ContentButtonViewModel();
			var page = new ContentButtonPage(inflator) { BindingContext = model };
			Assert.Same(page.caption, page.button.Content);
			Assert.Same(page.button, page.caption.Parent);
			Assert.Equal(model.Caption, page.caption.Text);
			Assert.Equal(model.Caption, SemanticProperties.GetDescription(page.button));
			Assert.Same(model.Action, page.button.Command);
			Assert.Equal(model.Parameter, page.button.CommandParameter);
			((IButton)page.button).Clicked();
			Assert.Equal(model.Parameter, model.ExecutedParameter);
		}

		[Theory]
		[XamlInflatorData]
		internal void StylesAndDynamicResources(XamlInflator inflator)
		{
			var page = new ContentButtonPage(inflator);
			Assert.Equal(new Thickness(1, 2, 3, 4), page.button.Padding);
			Assert.Equal(2, page.button.BorderWidth);
			Assert.Equal(8, page.button.CornerRadius);
			Assert.Equal(Colors.Red, page.button.BorderColor);
			Assert.Equal(Color.FromArgb("#40808080"), AndroidContentButton.GetRippleColor(page.button));
			page.Resources["ButtonBorder"] = Colors.Blue;
			Assert.Equal(Colors.Blue, page.button.BorderColor);
			AndroidContentButton.SetRippleColor(page.button, null);
			Assert.Null(AndroidContentButton.GetRippleColor(page.button));
		}
	}
}

public class ContentButtonViewModel
{
	public ContentButtonViewModel() => Action = new Command(parameter => ExecutedParameter = parameter);
	public string Caption => "Presentation content";
	public string Parameter => "parameter";
	public ICommand Action { get; }
	public object ExecutedParameter { get; private set; }
}
