using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Dispatching;
using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class EmbeddingTests
	{
		[Fact]
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
