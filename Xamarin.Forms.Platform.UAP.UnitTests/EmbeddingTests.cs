using System.Threading.Tasks;
using NUnit.Framework;
using Windows.UI.Xaml;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.Platform.UAP.UnitTests
{
	[TestFixture]
	public class EmbeddingTests
	{
		[Test]
		public async Task CanCreateFrameworkElementFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			await Device.InvokeOnMainThreadAsync(() => {
				FrameworkElement frameworkElement = contentPage.CreateFrameworkElement();
			});
		}
	}
}
