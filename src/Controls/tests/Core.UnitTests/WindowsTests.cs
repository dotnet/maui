using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class WindowsTests : BaseTestFixture
	{
		[Test]
		public void ContentPageFlowDirectionSetsOnIWindow()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			window.Page.FlowDirection = FlowDirection.RightToLeft;

			Assert.IsTrue((window as IWindow)
				.FlowDirection == FlowDirection.RightToLeft);
		}

		[Test]
		public void WindowFlowDirectionSetsOnPage()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			window.FlowDirection = FlowDirection.RightToLeft;

			Assert.IsTrue((window.Page as IFlowDirectionController)
				.EffectiveFlowDirection
				.IsRightToLeft());

			window.Page = new ContentPage();

			Assert.IsTrue((window.Page as IFlowDirectionController)
				.EffectiveFlowDirection
				.IsRightToLeft());
		}

		[Test]
		public void AddWindow()
		{
			var app = new TestApp();
			var window = app.CreateWindow();
			ValidateSetup(app);
		}

		[Test]
		public void SetMainPage()
		{
			var app = new Application();
			app.LoadPage(new ContentPage());
			ValidateSetup(app);
		}

		[Test]
		public void SetMainPageTwice()
		{
			var app = new Application();
			var firstPage = new ContentPage();
			var secondPage = new ContentPage();

			var wind1 = app.LoadPage(firstPage);
			var wind2 = app.LoadPage(secondPage);

			ValidateSetup(app, secondPage);
			Assert.IsNull(firstPage.Parent);
			Assert.AreEqual(wind1, wind2);
		}

		[Test]
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

		[Test]
		public void ListViewWindowIsInheritedByViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new View() }) };
			var window = new Window(new ContentPage { Content = lv });

			lv.ItemsSource = Enumerable.Range(0, 10);

			ViewCell cell = lv.TemplatedItems[0] as ViewCell;
			Assert.AreEqual(window, cell.View.Window);
		}

		[Test]
		public void ListViewWindowIsInheritedByLabelInViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new Label() }) };
			var cp = new ContentPage { Content = lv };
			var window = new Window(cp);

			Assert.AreEqual(window, lv.Window);
			Assert.AreEqual(window, cp.Window);

			lv.ItemsSource = Enumerable.Range(0, 10);

			var cell = lv.TemplatedItems[0] as ViewCell;

			Assert.AreEqual(window, cell.View.Window);
		}

		[Test]
		public void ListViewWindowIsInheritedByLayoutsInViewCells()
		{
			var lv = new ListView { ItemTemplate = new DataTemplate(() => new ViewCell { View = new Grid { new Label() } }) };
			var cp = new ContentPage { Content = lv };
			var window = new Window(cp);

			Assert.AreEqual(window, lv.Window);
			Assert.AreEqual(window, cp.Window);

			lv.ItemsSource = Enumerable.Range(0, 10);

			var cell = lv.TemplatedItems[0] as ViewCell;
			var grid = cell.View as Grid;
			var label = grid.Children[0] as Label;

			Assert.AreEqual(window, ((IWindowController)cell).Window);
			Assert.AreEqual(window, cell.View.Window);
			Assert.AreEqual(window, label.Window);
		}

		[Test]
		public void NestedControlsAllHaveTheSameWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			Assert.AreEqual(window, btn.Window);
			Assert.AreEqual(window, grid.Window);
			Assert.AreEqual(window, cp.Window);
		}

		[Test]
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

			Assert.AreEqual(window, btn.Window);
			Assert.AreEqual(window, grid.Window);
			Assert.AreEqual(window, cp.Window);
		}

		[Test]
		public void NestedControlsAllHaveTheSameWindowWhenAddedLater()
		{
			var btn = new Button();
			var grid = new Grid();
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			Assert.Null(btn.Window);
			Assert.AreEqual(window, grid.Window);
			Assert.AreEqual(window, cp.Window);

			grid.Children.Add(btn);

			Assert.AreEqual(window, btn.Window);
			Assert.AreEqual(window, grid.Window);
			Assert.AreEqual(window, cp.Window);
		}

		[Test]
		public void SwappingPagesUpdatesTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };

			var window = new Window(cp);
			var window2 = new Window(cp);

			Assert.AreEqual(window2, btn.Window);
			Assert.AreEqual(window2, grid.Window);
			Assert.AreEqual(window2, cp.Window);
		}

		[Test]
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

		[Test]
		public void DetachingInTheMiddleUnsetsTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			cp.Content = null;

			Assert.Null(btn.Window);
			Assert.Null(grid.Window);
			Assert.AreEqual(window, cp.Window);
		}

		[Test]
		public void RemovingControlsFromLayoutsUnsetsTheWindow()
		{
			var btn = new Button();
			var grid = new Grid { btn };
			var cp = new ContentPage { Content = grid };
			var window = new Window(cp);

			grid.Remove(btn);

			Assert.Null(btn.Window);
			Assert.AreEqual(window, grid.Window);
			Assert.AreEqual(window, cp.Window);
		}

		void ValidateSetup(Application app, Page page = null)
		{
			var window = (Window)app.Windows[0];
			page ??= window.Page;

			// Validate all the parent hierarchies are correct
			Assert.AreEqual(app, window.Parent);
			Assert.AreEqual(window, window.Page.Parent);
			Assert.AreEqual(app.Windows.Count, 1);
			Assert.AreEqual(app.LogicalChildren[0], window);
			Assert.AreEqual(window.LogicalChildren[0], page);
			Assert.AreEqual(app.LogicalChildren.Count, 1);
			Assert.AreEqual(window.LogicalChildren.Count, 1);
			Assert.AreEqual(app.NavigationProxy, window.NavigationProxy.Inner);
			Assert.AreEqual(window.NavigationProxy, page.NavigationProxy.Inner);
		}

		public class TestApp : Application
		{
			public TestWindow CreateWindow() =>
				(TestWindow)(this as IApplication).CreateWindow(null);

			protected override Window CreateWindow(IActivationState activationState)
			{
				return new TestWindow(new ContentPage());
			}
		}

		public class TestWindow : Window
		{
			public TestWindow(Page page) : base(page)
			{
			}
		}
	}
}
