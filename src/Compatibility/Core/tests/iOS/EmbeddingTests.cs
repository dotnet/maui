using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class EmbeddingTests
	{
		[Test]
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