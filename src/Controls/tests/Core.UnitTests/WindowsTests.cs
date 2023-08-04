using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
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
		public void AddingTabSetsWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(new TabbedPage() { Children = { cp } });

			Assert.Equal(window, btn.Window);
		}

		[Fact]
		public void RemovingTabUnSetsWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var tabbedPage = new TabbedPage() { Children = { cp, new ContentPage() } };
			var window = new Window(tabbedPage);

			Assert.Equal(window, btn.Window);
			tabbedPage.Children.RemoveAt(0);
			Assert.Null(btn.Window);
		}

		[Fact]
		public async Task PushingPageSetsWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var secondPage = new ContentPage { Content = grid };
			var np = new NavigationPage(new ContentPage());
			var window = new Window(np);
			await np.PushAsync(secondPage);

			Assert.Equal(window, btn.Window);
		}

		[Fact]
		public async Task PoppingPageUnsetsWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var secondPage = new ContentPage { Content = grid };
			var np = new NavigationPage(new ContentPage());
			var window = new Window(np);
			await np.PushAsync(secondPage);
			await np.PopAsync();

			Assert.Null(btn.Window);
		}

		[Fact]
		public void PoppingPageUnSetsWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var tabbedPage = new TabbedPage() { Children = { cp, new ContentPage() } };
			var window = new Window(tabbedPage);

			Assert.Equal(window, btn.Window);
			tabbedPage.Children.RemoveAt(0);
			Assert.Null(btn.Window);
		}

		[Fact]
		void DestroyedFiresDisappearingEvent()
		{
			int disappear = 0;
			int appear = 0;

			var cp = new ContentPage();
			IWindow window = new TestWindow(cp);
			window.Activated();

			cp.Appearing += (_, __) => appear++;
			cp.Disappearing += (_, __) => disappear++;

			window.Destroying();
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

			var app = window.Parent as TestApp;
			Assert.Equal(0, disappear);
			window.Destroying();

			// simulate platform requesting another window for the same page
			_ = app.CreateWindow(cp);

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
			Assert.Single(app.Windows);
			Assert.Equal(app.LogicalChildrenInternal[0], window);
			Assert.Equal(window.LogicalChildrenInternal[0], page);
			Assert.Single(app.LogicalChildrenInternal);
			Assert.Single(window.LogicalChildrenInternal);
			Assert.Single(window.LogicalChildrenInternal.OfType<Page>());
			Assert.Equal(app.NavigationProxy, window.NavigationProxy.Inner);
			Assert.Equal(window.NavigationProxy, page.NavigationProxy.Inner);
		}

		[Fact]
		public void SettingCoreFrameOnlyFiresEventOnce()
		{
			var sizeChangedCount = 0;
			var propertyChanges = new List<string>();

			var window = new TestWindow();
			window.SizeChanged += (sender, e) => sizeChangedCount++;
			window.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

			((IWindow)window).FrameChanged(new Rect(100, 200, 300, 400));

			Assert.Equal(1, sizeChangedCount);
			Assert.Equal(new[] { "X", "Y", "Width", "Height" }, propertyChanges);
		}

		[Fact]
		public void SettingSameCoreFrameDoesNothing()
		{
			var sizeChangedCount = 0;
			var propertyChanges = new List<string>();

			var window = new TestWindow();
			((IWindow)window).FrameChanged(new Rect(100, 200, 300, 400));

			window.SizeChanged += (sender, e) => sizeChangedCount++;
			window.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

			((IWindow)window).FrameChanged(new Rect(100, 200, 300, 400));

			Assert.Equal(0, sizeChangedCount);
			Assert.Empty(propertyChanges);
		}

		[Fact]
		public void UpdatingSingleCoordinateOnlyFiresSinglePropertyAndFrameEvent()
		{
			var sizeChangedCount = 0;
			var propertyChanges = new List<string>();

			var window = new TestWindow();
			((IWindow)window).FrameChanged(new Rect(100, 200, 300, 400));

			window.SizeChanged += (sender, e) => sizeChangedCount++;
			window.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

			((IWindow)window).FrameChanged(new Rect(100, 250, 300, 400));

			Assert.Equal(1, sizeChangedCount);
			Assert.Equal(new[] { "Y" }, propertyChanges);
		}

		[Fact]
		public void UpdatingSingleBoundOnlyFiresSingleProperty()
		{
			var sizeChangedCount = 0;
			var propertyChanges = new List<string>();

			var window = new TestWindow();
			((IWindow)window).FrameChanged(new Rect(100, 200, 300, 400));

			window.SizeChanged += (sender, e) => sizeChangedCount++;
			window.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

			((IWindow)window).FrameChanged(new Rect(100, 200, 350, 400));

			Assert.Equal(1, sizeChangedCount);
			Assert.Equal(new[] { "Width" }, propertyChanges);
		}

		[Fact]
		public void DefaultBoundsArePassedToCoreCorrectly()
		{
			var controlsWindow = new TestWindow();
			var coreWindow = controlsWindow as IWindow;

			Assert.Equal(double.NaN, coreWindow.X);
			Assert.Equal(double.NaN, coreWindow.Y);
			Assert.Equal(double.NaN, coreWindow.Width);
			Assert.Equal(double.NaN, coreWindow.Height);

			Assert.Equal(double.NaN, coreWindow.MinimumWidth);
			Assert.Equal(double.NaN, coreWindow.MinimumHeight);

			Assert.Equal(double.NaN, coreWindow.MaximumWidth);
			Assert.Equal(double.NaN, coreWindow.MaximumHeight);
		}

		[Fact]
		public void ShellTitleChangePropagatesToWindow()
		{
			var app = new TestApp();
			var shell = new ShellTestBase.TestShell() { Title = "test" };
			var window = app.CreateWindow();
			bool fired = false;
			window.Page = shell;
			window.Handler = new WindowHandlerStub(new PropertyMapper<IWindow, WindowHandlerStub>()
			{
				[nameof(IWindow.Title)] = (_, _) => fired = true
			});

			// reset after setting handler
			fired = false;
			shell.Title = "new title";


			Assert.Equal(shell.Title, (window as IWindow).Title);
			Assert.True(fired);
		}

		[Fact]
		public void PreviousShellDisconnectsFromWindowPropertyChanged()
		{
			var app = new TestApp();
			var oldShell = new Shell() { Title = "Old Shell" };
			var window = app.CreateWindow(oldShell);
			bool fired = false;

			window.Handler = new WindowHandlerStub(new PropertyMapper<IWindow, WindowHandlerStub>()
			{
				[nameof(IWindow.Title)] = (_, _) =>
				{
					fired = true;
				}
			});

			window.Page = new Shell() { Title = "test" };

			// reset after setting handler
			fired = false;

			oldShell.Title = "new title";
			Assert.Equal("test", (window as IWindow).Title);
			Assert.False(fired);
		}

		[Theory]
		[InlineData(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN)]
		[InlineData(-1, -1, -1, -1, -1, -1, double.NaN, double.NaN)]
		[InlineData(0, 0, 100, 100, 0, 0, 100, 100)]
		public void BoundsArePassedToCoreCorrectly(double inX, double inY, double inW, double inH, double outX, double outY, double outW, double outH)
		{
			var controlsWindow = new TestWindow
			{
				X = inX,
				Y = inY,
				Width = inW,
				Height = inH
			};

			var coreWindow = controlsWindow as IWindow;

			Assert.Equal(outX, coreWindow.X);
			Assert.Equal(outY, coreWindow.Y);
			Assert.Equal(outW, coreWindow.Width);
			Assert.Equal(outH, coreWindow.Height);
		}

		[Theory]
		[InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
		[InlineData(-1, -1, double.NaN, double.NaN)]
		[InlineData(100, 100, 100, 100)]
		[InlineData(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)]
		public void MaxDimensionsArePassedToCoreCorrectly(double inW, double inH, double outW, double outH)
		{
			var controlsWindow = new TestWindow
			{
				MaximumWidth = inW,
				MaximumHeight = inH
			};

			var coreWindow = controlsWindow as IWindow;

			Assert.Equal(outW, coreWindow.MaximumWidth);
			Assert.Equal(outH, coreWindow.MaximumHeight);
		}

		[Theory]
		[InlineData(double.NaN, double.NaN, double.NaN, double.NaN)]
		[InlineData(-1, -1, double.NaN, double.NaN)]
		[InlineData(100, 100, 100, 100)]
		[InlineData(0, 0, 0, 0)]
		public void MinDimensionsArePassedToCoreCorrectly(double inW, double inH, double outW, double outH)
		{
			var controlsWindow = new TestWindow
			{
				MinimumWidth = inW,
				MinimumHeight = inH
			};

			var coreWindow = controlsWindow as IWindow;

			Assert.Equal(outW, coreWindow.MinimumWidth);
			Assert.Equal(outH, coreWindow.MinimumHeight);
		}

		[Fact]
		public async Task WindowDoesNotLeak()
		{
			var application = new Application();
			WeakReference reference;

			// Scope for window
			{
				var window = new Window { Page = new ContentPage() };
				reference = new WeakReference(window);
				application.OpenWindow(window);
				((IWindow)window).Destroying();
			}

			// GC
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(reference.IsAlive, "Window should not be alive!");
		}

		// NOTE: this test is here to show `ConditionalWeakTable _requestedWindows` was a bad idea
		[Fact]
		public async Task TwoKeysSameWindow()
		{
			var application = new Application();
			var window = new Window { Page = new ContentPage() };

			application.OpenWindow(window);

			// Access Application._requestedWindows
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var table = typeof(Application).GetField("_requestedWindows", flags).GetValue(application) as IDictionary;
			Assert.NotNull(table);

			// A "cloned" key should still work
			string key;
			{
				var originalKey = table.Keys.OfType<string>().Single();
				key = new string(originalKey);
				Assert.NotSame(originalKey, key);
			}

			// GC collect the original key
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Same window, doesn't create a new one
			var actual = ((IApplication)application).CreateWindow(new ActivationState(new MockMauiContext(), new PersistedState
			{
				{ Application.MauiWindowIdKey, key }
			}));
			Assert.Same(window, actual);
			Assert.Empty(table);
		}
	}
}
