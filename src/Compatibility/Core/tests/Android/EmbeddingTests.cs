using System.Threading.Tasks;
using AndroidX.Fragment.App;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class EmbeddingTests : PlatformTestFixture
	{
		[Test]
		public async Task CanCreateFragmentFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			contentPage.Parent = Application.Current;
			await contentPage.Dispatcher.DispatchAsync(() =>
			{
				Fragment fragment = contentPage.CreateSupportFragment(Context);
			});
			contentPage.Parent = null;
		}
	}
}