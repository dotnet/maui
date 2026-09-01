using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using ContentView = Microsoft.Maui.Controls.ContentView;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElementTree)]
#if ANDROID || IOS || MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
#endif
#if IOS || MACCATALYST
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationRenderer)] // See RendererHandlerVariant.cs
#endif
	public partial class VisualElementTreeTests : ControlsHandlerTestBase
	{
		protected virtual void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.SetupShellHandlers();

				builder.ConfigureMauiHandlers(handlers =>
				{
					RegisterNavigationPageHandler(handlers);
					handlers.AddHandler<NestingView, NestingViewHandler>();
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Border, BorderHandler>();
				});
			});
		}

		// Extracted so an iOS/MacCatalyst-only subclass can swap in NavigationViewHandler,
		// letting every VisualElementTreeTests test run against both the NavigationPage renderer
		// and handler. See VisualElementTreeNavigationHandlerTests.iOS.cs and
		// RendererHandlerVariant.cs.
		protected virtual void RegisterNavigationPageHandler(IMauiHandlersCollection handlers)
		{
#if IOS || MACCATALYST
			handlers.AddHandler(typeof(Controls.NavigationPage), typeof(Controls.Handlers.Compatibility.NavigationRenderer));
#else
			handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
#endif
		}

#if IOS || MACCATALYST
		[Fact]
		public async Task Handler_GetVisualTreeElements()
		{
			// Handler-only: always exercises NavigationViewHandler, even when this test class
			// is run as the base VisualElementTreeTests (Renderer-default) suite, since there is
			// no separate handler-only subclass test for this scenario. Bypasses
			// SetupBuilder/RegisterNavigationPageHandler so the subclass's default can't affect it.
			EnsureHandlerCreated(builder =>
			{
				builder.SetupShellHandlers();

				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<NestingView, NestingViewHandler>();
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Border, BorderHandler>();
				});
			});

			var border = new Border() { WidthRequest = 50, HeightRequest = 50, StrokeShape = new RoundRectangle() { CornerRadius = 5 } };
			var label = new Label() { Text = "Find Me" };

			var page = new ContentPage() { Title = "Title Page" };
			page.Content = new VerticalStackLayout()
			{
				label,
				border
			};

			var rootPage = await InvokeOnMainThreadAsync(() =>
				new NavigationPage(page)
			);

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage, async handler =>
			{
				// Handler path: NavigationPage frame may not fire BatchCommitted in time,
				// so wait for the content page to be navigated and loaded first.
				await OnNavigatedToAsync(page);
				await OnLoadedAsync(page.Content);

				await OnFrameSetToNotEmpty(border);
				await OnFrameSetToNotEmpty(label);

				var locationOnScreen = label.GetLocationOnScreen().Value;
				var labelFrame = label.Frame;
				var window = rootPage.Window;

				var topLeft = new Graphics.Point(locationOnScreen.X + 1, locationOnScreen.Y + 1);
				Assert.True(window.GetVisualTreeElements(topLeft).Contains(label), $"Unable to find label using top left coordinate: {topLeft} with label location: {label.GetBoundingBox()}");

				var bottomRight = new Graphics.Point(
					locationOnScreen.X + labelFrame.Width - 1,
					locationOnScreen.Y + labelFrame.Height - 1);
				Assert.True(window.GetVisualTreeElements(bottomRight).Contains(label), $"Unable to find label using bottom right coordinate: {bottomRight} with label location: {label.GetBoundingBox()}");

				Assert.DoesNotContain(label, window.GetVisualTreeElements(
						locationOnScreen.X + labelFrame.Width + 1,
						locationOnScreen.Y + labelFrame.Height + 1
					));
			});
		}
#endif

		[Fact]
		public async Task GetVisualTreeElements()
		{
#if IOS || MACCATALYST
			// Renderer-only: this test forces the old event-based NavigationImpl path
			// (setForMaui:false) below, which NavigationRenderer supports but
			// NavigationViewHandler does not implement via RequestNavigation (causes hangs).
			// Register NavigationRenderer directly (not via SetupBuilder/RegisterNavigationPageHandler)
			// so this stays Renderer-only even when inherited by VisualElementTreeNavigationHandlerTests.
			EnsureHandlerCreated(builder =>
			{
				builder.SetupShellHandlers();

				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(Controls.Handlers.Compatibility.NavigationRenderer));
					handlers.AddHandler<NestingView, NestingViewHandler>();
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Border, BorderHandler>();
				});
			});
#else
			SetupBuilder();
#endif

			var border = new Border() { WidthRequest = 50, HeightRequest = 50, StrokeShape = new RoundRectangle() { CornerRadius = 5 } };
			var label = new Label() { Text = "Find Me" };

			var page = new ContentPage() { Title = "Title Page" };
			page.Content = new VerticalStackLayout()
			{
				label,
				border
			};

			var rootPage = await InvokeOnMainThreadAsync(() =>
#if IOS || MACCATALYST
				// Use setForMaui:false to force old event-based NavigationImpl path.
				// NavigationRenderer doesn't implement RequestNavigation, causing hangs.
				new NavigationPage(false, page)
#else
				new NavigationPage(page)
#endif
			);

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage, async handler =>
			{
				await OnFrameSetToNotEmpty(rootPage);
				await OnFrameSetToNotEmpty(border);
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();
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
			SetupBuilder();

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
