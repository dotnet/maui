using System.Threading.Tasks;
using Android.Support.V4.App;
using NUnit.Framework;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	[TestFixture]
	public class EmbeddingTests : PlatformTestFixture
	{
		[Test]
		public async Task CanCreateFragmentFromContentPage()
		{
			var contentPage = new ContentPage { Title = "Embedded Page" };
			contentPage.Parent = Application.Current;
			await Device.InvokeOnMainThreadAsync(() => {
				Fragment fragment = contentPage.CreateSupportFragment(Context);
			});
			contentPage.Parent = null;
		}
	}
}