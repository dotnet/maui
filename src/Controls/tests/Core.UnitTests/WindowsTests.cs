using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class WindowsTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
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
