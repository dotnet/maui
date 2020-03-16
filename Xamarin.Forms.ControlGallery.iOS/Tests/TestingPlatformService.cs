using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.iOS.Platform.CreateRenderer(visualElement));
		}
	}
}