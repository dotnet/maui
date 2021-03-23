using System.Threading.Tasks;
using Android.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() =>
				Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.CreateRendererWithContext(visualElement,
					DependencyService.Resolve<Context>()));

			await Task.CompletedTask;
		}
	}
}