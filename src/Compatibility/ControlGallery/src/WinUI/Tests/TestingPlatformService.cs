using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests;
using Microsoft.Maui.Dispatching;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await visualElement.Dispatcher.DispatchAsync(() => Platform.UWP.Platform.CreateRenderer(visualElement));
		}
	}
}
