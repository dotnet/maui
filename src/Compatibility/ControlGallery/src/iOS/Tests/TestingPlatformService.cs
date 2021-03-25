using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.iOS.Platform.CreateRenderer(visualElement));
		}
	}
}