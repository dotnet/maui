using System.Threading.Tasks;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal.Tests;
using System.Maui.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace System.Maui.ControlGallery.WindowsUniversal.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.UWP.Platform.CreateRenderer(visualElement));
		}
	}
}
