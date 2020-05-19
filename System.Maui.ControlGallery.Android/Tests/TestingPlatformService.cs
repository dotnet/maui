using System.Threading.Tasks;
using Android.Content;
using System.Maui;
using System.Maui.ControlGallery.Android.Tests;
using System.Maui.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace System.Maui.ControlGallery.Android.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => 
				Platform.Android.Platform.CreateRendererWithContext(visualElement,
					DependencyService.Resolve<Context>()));
		}
	}
}