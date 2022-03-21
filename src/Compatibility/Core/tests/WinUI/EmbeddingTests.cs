using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Dispatching;
using Microsoft.UI.Xaml;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class EmbeddingTests
	{
		[Test]
		public async Task CanCreateFrameworkElementFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			await contentPage.Dispatcher.DispatchAsync(() =>
			{
				FrameworkElement frameworkElement = contentPage.CreateFrameworkElement();
			});
		}
	}
}
