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
	public partial class VisualElementTreeTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.SetupShellHandlers();

				builder.ConfigureMauiHandlers(handlers =>
				{
#if IOS || MACCATALYST
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(Controls.Handlers.Compatibility.NavigationRenderer));
#else
					handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
#endif
					handlers.AddHandler<NestingView, NestingViewHandler>();
					handlers.AddHandler<ContentView, ContentViewHandler>();
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Border, BorderHandler>();
				});
			});
		}

		[Fact]
		public async Task GetVisualTreeElements()
		{
			SetupBuilder();

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
