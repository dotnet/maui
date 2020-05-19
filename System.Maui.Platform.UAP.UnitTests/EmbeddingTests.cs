using System.Threading.Tasks;
using NUnit.Framework;
using global::Windows.UI.Xaml;
using System.Maui.Platform.UWP;

namespace System.Maui.Platform.UAP.UnitTests
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
