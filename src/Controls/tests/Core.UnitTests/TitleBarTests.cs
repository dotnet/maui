using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TitleBarTests : BaseTestFixture
	{
		[Fact, Category(TestCategory.Memory)]
		public async Task TitleBarDoesNotLeak()
		{
			var application = new Application();

			WeakReference CreateReference()
			{
				var window = new Window { Page = new ContentPage() };
				var firstTitleBar = new TitleBar();
				var secondTitleBar = new TitleBar();
				var reference = new WeakReference(firstTitleBar);

				window.TitleBar = firstTitleBar;

				application.OpenWindow(window);

				window.TitleBar = secondTitleBar;

				((IWindow)window).Destroying();
				return reference;
			}

			var reference = CreateReference();

			// GC
			await TestHelpers.Collect();

			Assert.False(reference.IsAlive, "TitleBar should not be alive!");

			GC.KeepAlive(application);
		}
	}
}