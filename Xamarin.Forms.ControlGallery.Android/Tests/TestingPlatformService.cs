using System.Threading.Tasks;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Xamarin.Forms.ControlGallery.Android.Tests
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