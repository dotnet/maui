using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.iOS.Tests;
using Microsoft.Maui.Controls.ControlGallery.Tests;
using Microsoft.Maui.Dispatching;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Dependency(typeof(TestingPlatformService))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.iOS.Tests
{
	[System.Obsolete]
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await visualElement.Dispatcher.DispatchAsync(() => Platform.iOS.Platform.CreateRenderer(visualElement));
		}
	}
}