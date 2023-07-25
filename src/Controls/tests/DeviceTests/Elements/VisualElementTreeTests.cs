using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElementTree)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class VisualElementTreeTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.Shell), typeof(ShellHandler));
#if IOS || MACCATALYST
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(Controls.Handlers.Compatibility.NavigationRenderer));
#else
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
#endif
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
					handlers.AddHandler<MenuBar, MenuBarHandler>();
					handlers.AddHandler<MenuBarItem, MenuBarItemHandler>();
					handlers.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
					handlers.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
#if WINDOWS
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
#endif
				});
			});
		}

		[Fact]
		public async Task GetVisualTreeElements()
		{
			SetupBuilder();
			var label = new Label() { Text = "Find Me" };
			var page = new ContentPage() { Title = "Title Page" };
			page.Content = new VerticalStackLayout()
			{
				label
			};

			var rootPage = await InvokeOnMainThreadAsync(() =>
				new NavigationPage(page)
			);

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage, async handler =>
			{
				await OnFrameSetToNotEmpty(label);
				var locationOnScreen = label.GetLocationOnScreen().Value;
				var labelFrame = label.Frame;
				var window = rootPage.Window;

				// Find label at the top left corner
				var topLeft = new Graphics.Point(locationOnScreen.X + 1, locationOnScreen.Y + 1);

				Assert.True(window.GetVisualTreeElements(topLeft).Contains(label), $"Unable to find label using top left coordinate: {topLeft} with label location: {label.GetBoundingBox()}");

				// find label at the bottom right corner
				var bottomRight = new Graphics.Point(
					locationOnScreen.X + labelFrame.Width - 1,
					locationOnScreen.Y + labelFrame.Height - 1);

				Assert.True(window.GetVisualTreeElements(bottomRight).Contains(label), $"Unable to find label using bottom right coordinate: {bottomRight} with label location: {label.GetBoundingBox()}");

				// Ensure that the point directly outside the bounds of the label doesn't
				// return the label
				Assert.DoesNotContain(label, window.GetVisualTreeElements(
						locationOnScreen.X + labelFrame.Width + 1,
						locationOnScreen.Y + labelFrame.Height + 1
					));

			});
		}
	}
}
