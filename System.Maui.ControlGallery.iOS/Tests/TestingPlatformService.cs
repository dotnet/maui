using System.Threading.Tasks;
using System.Maui;
using System.Maui.ControlGallery.iOS.Tests;
using System.Maui.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace System.Maui.ControlGallery.iOS.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.iOS.Platform.CreateRenderer(visualElement));
		}
	}
}