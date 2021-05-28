using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
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
			await Device.InvokeOnMainThreadAsync(() =>
			{
				FrameworkElement frameworkElement = contentPage.CreateFrameworkElement();
			});
		}
	}
}
