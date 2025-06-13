using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Xunit;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class EmbeddingTests
	{
		[Fact]
		public async Task CanCreateViewControllerFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			await contentPage.Dispatcher.DispatchAsync(() =>
			{
				UIViewController controller = contentPage.CreateViewController();
			});
		}
	}
}