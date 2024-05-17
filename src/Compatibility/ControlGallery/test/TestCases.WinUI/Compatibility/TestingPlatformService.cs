using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.Tests;
using Microsoft.Maui.Controls.ControlGallery.WinUI.Tests;
using Microsoft.Maui.Dispatching;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.ControlGallery.WinUI.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			await visualElement.Dispatcher.DispatchAsync(() => Platform.UWP.Platform.CreateRenderer(visualElement));
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}
}
