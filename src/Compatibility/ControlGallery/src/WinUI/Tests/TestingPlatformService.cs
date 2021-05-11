using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.UWP.Platform.CreateRenderer(visualElement));
		}
	}
}
