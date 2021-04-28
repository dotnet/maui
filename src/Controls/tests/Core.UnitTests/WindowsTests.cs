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
			var app = new TestApp();
			app.MainPage = new ContentPage();
			ValidateSetup(app);
		}

		[Test]
		public void SetMainPageTwice()
		{
			var app = new TestApp();
			var firstPage = new ContentPage();
			var secondPage = new ContentPage();

			app.MainPage = firstPage;
			app.MainPage = secondPage;

			ValidateSetup(app, secondPage);
			Assert.IsNull(firstPage.Parent);
		}


		void ValidateSetup(TestApp app, Page page = null)
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

			protected override IWindow CreateWindow(IActivationState activationState)
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
