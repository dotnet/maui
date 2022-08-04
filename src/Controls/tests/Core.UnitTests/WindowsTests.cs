using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	
	public class WindowsTests : BaseTestFixture
	{
		[Fact]
		public void ContentPageFlowDirectionSetsOnIWindow()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			window.Page.FlowDirection = FlowDirection.RightToLeft;

			Assert.True((window as IWindow)
				.FlowDirection == FlowDirection.RightToLeft);
		}

		[Fact]
		public void WindowFlowDirectionSetsOnPage()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			window.FlowDirection = FlowDirection.RightToLeft;

			Assert.True((window.Page as IFlowDirectionController)
				.EffectiveFlowDirection
				.IsRightToLeft());

			window.Page = new ContentPage();

			Assert.True((window.Page as IFlowDirectionController)
				.EffectiveFlowDirection
				.IsRightToLeft());
		}

		[Fact]
		public void AddWindow()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			ValidateSetup(app);
		}

		[Fact]
		public void SetMainPage()
		{
			var app = new Application();
			app.LoadPage(new ContentPage());
			ValidateSetup(app);
		}

		[Fact]
		public void SetMainPageTwice()
		{
			var app = new Application();
			var firstPage = new ContentPage();
			var secondPage = new ContentPage();

			var wind1 = app.LoadPage(firstPage);
			var wind2 = app.LoadPage(secondPage);

			ValidateSetup(app, secondPage);
			Assert.Null(firstPage.Parent);
			Assert.Equal(wind1, wind2);
		}

		[Fact]
		public void AddAndRemoveVisualDiagnosticAdorner()
		{
			var app = new Application();
			var contentPage = new ContentPage();
			var wind1 = app.LoadPage(contentPage);
			ValidateSetup(app);
			var visualElement = contentPage as IVisualTreeElement;
			Assert.True(wind1.VisualDiagnosticsOverlay.AddAdorner(visualElement, false));
			Assert.True(wind1.VisualDiagnosticsOverlay.WindowElements.Count > 0);
			// Can't add existing IVisualTreeElement twice.
			Assert.False(wind1.VisualDiagnosticsOverlay.AddAdorner(visualElement, false));

			var adorner = wind1.VisualDiagnosticsOverlay.WindowElements.First() as IAdorner;

			// Can't add existing Adorner twice.
			Assert.False(wind1.VisualDiagnosticsOverlay.AddAdorner(adorner, false));

			Assert.True(wind1.VisualDiagnosticsOverlay.RemoveAdorner(adorner));

			Assert.True(wind1.VisualDiagnosticsOverlay.WindowElements.Count == 0);
		}

		[Fact]
		public void WindowCanRetrieveDisplayDensity()
		{
			var window = new Window(new ContentPage());
			var handler = new WindowHandlerStub(commandMapper: new CommandMapper<IWindow, WindowHandlerStub>(WindowHandlerStub.CommandMapper)
			{
				[nameof(IWindow.RequestDisplayDensity)] = (h, v, a) => ((DisplayDensityRequest)a).SetResult(42)
			});

			window.Handler = handler;

			Assert.Equal(42, window.DisplayDensity);
		}

		[Fact]
		public void ListViewWindowIsInheritedByViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new View() }) };
			var window = new Window(new ContentPage { Content = lv });

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			Assert.Equal(window, cell.View.Window);
		}

		[Fact]
		public void ListViewWindowIsInheritedByLabelInViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label() }) };
			var cp = new ContentPage { Content = lv };
			var window = new Window(cp);

			Assert.Equal(window, lv.Window);
			Assert.Equal(window, cp.Window);

			lv.ItemsSource = Enumerable.Range(0, 10);

			var cell = lv.TemplatedItems[0] as ViewCell;

			Assert.Equal(window, cell.View.Window);
		}

		[Fact]
		public void ListViewWindowIsInheritedByLayoutsInViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new Grid { new Label() } }) };
			var cp = new ContentPage { Content = lv };
			var window = new Window(cp);

			Assert.Equal(window, lv.Window);
			Assert.Equal(window, cp.Window);

			lv.ItemsSource = Enumerable.Range(0, 10);

			var cell = lv.TemplatedItems[0] as ViewCell;
			var grid = cell.View as Grid;
			var label = grid.Children[0] as Label;

			Assert.Equal(window, ((IWindowController)cell).Window);
			Assert.Equal(window, cell.View.Window);
			Assert.Equal(window, label.Window);
		}

		[Fact]
		public void NestedControlsAllHaveTheSameWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			Assert.Equal(window, btn.Window);
			Assert.Equal(window, grid.Window);
			Assert.Equal(window, cp.Window);
		}

		[Fact]
		public void PageHasTheSameWindowWhenAddedLater()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window();

			Assert.Null(btn.Window);
			Assert.Null(grid.Window);
			Assert.Null(cp.Window);

			window.Page = cp;

			Assert.Equal(window, btn.Window);
			Assert.Equal(window, grid.Window);
			Assert.Equal(window, cp.Window);
		}

		[Fact]
		public void NestedControlsAllHaveTheSameWindowWhenAddedLater()
		{
			var btn = new Button();
			var grid = new Grid();
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			Assert.Null(btn.Window);
			Assert.Equal(window, grid.Window);
			Assert.Equal(window, cp.Window);

			grid.Children.Add(btn);

			Assert.Equal(window, btn.Window);
			Assert.Equal(window, grid.Window);
			Assert.Equal(window, cp.Window);
		}

		[Fact]
		public void SwappingPagesUpdatesTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };

			var window = new Window(cp);
			var window2 = new Window(cp);

			Assert.Equal(window2, btn.Window);
			Assert.Equal(window2, grid.Window);
			Assert.Equal(window2, cp.Window);
		}

		[Fact]
		public void DetachingThePageUnsetsTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			window.Page = null;

			Assert.Null(btn.Window);
			Assert.Null(grid.Window);
			Assert.Null(cp.Window);
		}

		[Fact]
		public void DetachingInTheMiddleUnsetsTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			cp.Content = null;

			Assert.Null(btn.Window);
			Assert.Null(grid.Window);
			Assert.Equal(window, cp.Window);
		}

		[Fact]
		public void RemovingControlsFromLayoutsUnsetsTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			grid.Remove(btn);

			Assert.Null(btn.Window);
			Assert.Equal(window, grid.Window);
			Assert.Equal(window, cp.Window);
		}

		[Fact]
		public void ApplicationIsSetOnWindowBeforeAppearingIsCalledOnPage()
		{
			bool passed = false;
			ContentPage cp = new ContentPage();
			cp.Appearing += (_, _) =>
			{
				var findApplication = cp?.Parent?.Parent as IApplication;
				Assert.NotNull(findApplication);
				passed = true;
			};

			_ = new TestApp().CreateWindow(cp);

			Assert.True(passed);
		}

		[Fact]
		void DeActivatedFiresDisappearingEvent()
		{
			int disappear = 0;
			int appear = 0;

			var cp = new ContentPage();
			IWindow window = new TestWindow(cp);
			window.Activated();

			cp.Appearing += (_, __) => appear++;
			cp.Disappearing += (_, __) => disappear++;

			window.Deactivated();
			Assert.Equal(1, disappear);
			Assert.Equal(0, appear);
		}

		[Fact]
		public void ReActivatedFiresCorrectActivatedEvent()
		{
			int disappear = 0;
			int appear = 0;

			var cp = new ContentPage();
			IWindow window = new TestWindow(cp);
			window.Activated();

			cp.Appearing += (_, __) => appear++;
			cp.Disappearing += (_, __) => disappear++;

			Assert.Equal(0, disappear);
			window.Deactivated();
			window.Activated();
			Assert.Equal(1, disappear);
			Assert.Equal(1, appear);
		}

		[Fact]
		public void RemovedPageFiresDisappearing()
		{
			int disappear = 0;
			int appear = 0;

			var cp = new ContentPage();
			cp.Disappearing += (_, __) => disappear++;

			Window window = new TestWindow(cp);
			(window as IWindow).Activated();
			Assert.Equal(0, disappear);
			window.Page = new ContentPage();
			Assert.Equal(1, disappear);
		}

		void ValidateSetup(Application app, Page page = null)
		{
			var window = (Window)app.Windows[0];
			page ??= window.Page;

			// Validate all the parent hierarchies are correct
			Assert.Equal(app, window.Parent);
			Assert.Equal(window, window.Page.Parent);
			Assert.Equal(1, app.Windows.Count);
			Assert.Equal(app.LogicalChildren[0], window);
			Assert.Equal(window.LogicalChildren[0], page);
			Assert.Single(app.LogicalChildren);
			Assert.Single(window.LogicalChildren);
			Assert.Equal(app.NavigationProxy, window.NavigationProxy.Inner);
			Assert.Equal(window.NavigationProxy, page.NavigationProxy.Inner);
		}
	}
}
