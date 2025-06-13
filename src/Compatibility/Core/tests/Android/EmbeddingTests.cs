using System.Threading.Tasks;
using AndroidX.Fragment.App;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class EmbeddingTests : PlatformTestFixture
	{
		[Fact]
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