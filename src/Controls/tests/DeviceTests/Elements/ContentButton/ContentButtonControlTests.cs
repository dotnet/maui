using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonControlTests : ControlsHandlerTestBase
	{
		void SetupBuilder() => EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<ContentButton, ContentButtonHandler>();
			handlers.AddHandler<Label, LabelHandler>();
			handlers.AddHandler<Grid, LayoutHandler>();
			handlers.AddHandler<ContentPresenter, ContentViewHandler>();
		}));

		[Fact]
		public async Task NestedContentAndTemplateCanBeReplaced()
		{
			SetupBuilder();
			await InvokeOnMainThreadAsync(() =>
			{
				var context = new object();
				var child = new Label { Text = "First" };
				var grid = new Grid { child };
				var button = new ContentButton { Content = grid, BindingContext = context };
				var handler = CreateHandler<ContentButtonHandler>(button);
				Assert.NotNull(child.Handler);
				Assert.Same(context, child.BindingContext);

				var replacement = new Label { Text = "Second" };
				button.Content = replacement;
				Assert.Null(grid.Parent);
				Assert.Same(button, replacement.Parent);
				Assert.NotNull(replacement.Handler);
				Assert.Same(context, replacement.BindingContext);

				button.ControlTemplate = new ControlTemplate(() => new ContentPresenter());
				var presenter = Assert.IsType<ContentPresenter>(((IContentView)button).PresentedContent);
				Assert.Same(replacement, presenter.Content);
				Assert.NotNull(presenter.Handler);
				button.ControlTemplate = null;
				button.Content = null;
				Assert.Null(replacement.Parent);
				Assert.Null(((IContentView)button).PresentedContent);
				((IElementHandler)handler).DisconnectHandler();
			});
		}

		[Fact]
		public async Task ChildTextChangeInvalidatesAndResizesButton()
		{
			SetupBuilder();
			var child = new Label { Text = "A" };
			var button = new ContentButton
			{
				Content = child,
				Padding = 8,
				BorderWidth = 2,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start,
			};
			await CreateHandlerAndAddToWindow(new Grid { button }, async () =>
			{
				await AssertionExtensions.AssertEventually(() => button.Width > 20 && button.Height > 20);
				var before = button.Width;
				child.Text = "A much longer presentation label";
				await AssertionExtensions.AssertEventually(() => button.Width > before);
				Assert.True(button.Height >= 20);
				child.Text = "A";
				await AssertionExtensions.AssertEventually(() => button.Width <= before + 1);
			});
		}

		[Fact]
		public async Task StylingAndEnabledStateCanUpdateWithAttachedContent()
		{
			SetupBuilder();
			var child = new Label { Text = "Presentation" };
			var button = new ContentButton { Content = child, WidthRequest = 120, HeightRequest = 80 };
			await CreateHandlerAndAddToWindow(button, () =>
			{
				button.Padding = new Thickness(1, 2, 3, 4);
				button.BorderWidth = 3;
				button.BorderColor = Colors.Red;
				button.CornerRadius = 12;
				button.Background = new SolidColorBrush(Colors.Blue);
				button.IsEnabled = false;
				Assert.False(child.IsEnabled);
				button.IsEnabled = true;
				Assert.True(child.IsEnabled);
				var size = ((IView)button).Measure(120, 80);
				Assert.Equal(120, size.Width, 1);
				Assert.Equal(80, size.Height, 1);
			});
		}
	}
}
