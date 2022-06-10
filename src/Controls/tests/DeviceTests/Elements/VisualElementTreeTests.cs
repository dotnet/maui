using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
#if ANDROID || IOS
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElementTree)]
#if ANDROID || IOS
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class VisualElementTreeTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.Shell), typeof(ShellHandler));
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

			var shell = await InvokeOnMainThreadAsync(() =>
				new Shell() { CurrentItem = new FlyoutItem() { Items = { page } } }
			);

			await CreateHandlerAndAddToWindow<IWindowHandler>(shell, async handler =>
			{
				await OnFrameSetToNotEmpty(label);
				var locationOnScreen = label.GetLocationOnScreen().Value;
				var labelFrame = label.Frame;
				var window = shell.Window;

				// Find label at the top left corner
				Assert.Contains(label, window.GetVisualTreeElements(locationOnScreen.X, locationOnScreen.Y));

				// find label at the bottom right corner
				Assert.Contains(label, window.GetVisualTreeElements(
						locationOnScreen.X + labelFrame.Width - 0.2,
						locationOnScreen.Y + labelFrame.Height - 0.2
					));

				// Ensure that the point directly outside the bounds of the label doesn't
				// return the label
				Assert.DoesNotContain(label, window.GetVisualTreeElements(
						locationOnScreen.X + labelFrame.Width + 0.2,
						locationOnScreen.Y + labelFrame.Height + 0.2
					));

			});
		}
	}
}