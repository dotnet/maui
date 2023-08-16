using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using System.Collections.Generic;
using ContentView = Microsoft.Maui.Controls.ContentView;
using Microsoft.Maui.Controls.Handlers.Items;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

#if IOS || MACCATALYST
using NavigationViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.NavigationRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElementTree)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
	public partial class VisualElementTreeTests : ControlsHandlerTestBase
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Controls.Shell, ShellHandler>();
					handlers.AddHandler<Controls.NavigationPage, NavigationViewHandler>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Toolbar, ToolbarHandler>();
					handlers.AddHandler<MenuBar, MenuBarHandler>();
					handlers.AddHandler<MenuBarItem, MenuBarItemHandler>();
					handlers.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
					handlers.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
					handlers.AddHandler<NestingView, NestingViewHandler>();
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
#if WINDOWS
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
#endif
				});

		[Fact]
		public async Task GetVisualTreeElements()
		{
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

		[Fact]
		public async Task FindPlatformViewInsideLayout()
		{
			var button = new Button();
			VerticalStackLayout views = new VerticalStackLayout()
			{
				new VerticalStackLayout()
				{
					button
				}
			};

			await CreateHandlerAndAddToWindow(views, () =>
			{
				var platformView = button.ToPlatform();
				var foundTreeElement = button.ToPlatform().GetVisualTreeElement();

				Assert.Equal(button, foundTreeElement);
			});
		}

		[Fact]
		public async Task FindPlatformViewInsideScrollView()
		{
			var button = new Button();
			ScrollView view = new ScrollView()
			{
				Content = button
			};

			await CreateHandlerAndAddToWindow(view, () =>
			{
				var platformView = button.ToPlatform();
				var foundTreeElement = button.ToPlatform().GetVisualTreeElement();

				Assert.Equal(button, foundTreeElement);
			});
		}

		[Fact]
		public async Task FindPlatformViewViaDefaultContainer()
		{
			var button = new Button();
			NestingView view = new NestingView();
			view.AddLogicalChild(button);

			await CreateHandlerAndAddToWindow(view, () =>
			{
				var platformView = button.ToPlatform();
				var foundTreeElement = button.ToPlatform().GetVisualTreeElement();

				Assert.Equal(button, foundTreeElement);
			});
		}

		[Fact]
		public async Task FindVisualTreeElementWithArbitraryPlatformViewsAdded()
		{
			var button = new Button();
			NestingView view = new NestingView();

			await CreateHandlerAndAddToWindow<NestingViewHandler>(view, (handler) =>
			{
				handler
					.PlatformView
					.AddChild()
					.AddChild()
					.AddChild()
					.AddChild(button, view);

				var platformView = button.ToPlatform();
				var foundTreeElement = button.ToPlatform().GetVisualTreeElement();

				Assert.Equal(button, foundTreeElement);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task FindFirstMauiParentElement(bool searchAncestors)
		{
			var viewToLocate = new NestingView();
			NestingView view = new NestingView();

			await CreateHandlerAndAddToWindow<NestingViewHandler>(view, (handler) =>
			{
				var nestedChild =
					handler.PlatformView
						.AddChild<NestingViewPlatformView>(viewToLocate, view)
						.AddChild()
						.AddChild()
						.AddChild();

				var foundTreeElement = nestedChild.GetVisualTreeElement(searchAncestors);

				if (searchAncestors)
					Assert.Equal(viewToLocate, foundTreeElement);
				else
					Assert.Null(foundTreeElement);
			});
		}

		[Theory]
		[ClassData(typeof(FindVisualTreeElementInsideTestCases))]
		public async Task FindPlatformViewInsideView(FindVisualTreeElementInsideTestCase testCase)
		{
			VisualElement rootView;
			VisualElement viewToLocate;

			(rootView, viewToLocate) = testCase.CreateVisualElement();
			await CreateHandlerAndAddToWindow(rootView, () =>
			{
				var platformView = viewToLocate.ToPlatform();
				var foundTreeElement = platformView.GetVisualTreeElement();
				Assert.Equal(viewToLocate, foundTreeElement);
			});
		}
	}
}
