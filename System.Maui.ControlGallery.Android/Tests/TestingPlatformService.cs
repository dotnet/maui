using System.Maui;
using System.Maui.ControlGallery.Android.Tests;
using System.Maui.Controls.Tests;
using System.Threading.Tasks;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace System.Maui.ControlGallery.Android.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			//await Device.InvokeOnMainThreadAsync(() =>
			//	Platform.Android.AppCompat.Platform.CreateRendererWithContext(visualElement,
			//		DependencyService.Resolve<Context>()));

			await Task.CompletedTask;
		}
	}
}