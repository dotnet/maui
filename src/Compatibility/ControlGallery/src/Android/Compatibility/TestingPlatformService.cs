using System.Threading.Tasks;
using Android.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.Android.Tests;
using Microsoft.Maui.Controls.ControlGallery.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.ControlGallery.Android.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
#pragma warning disable CS0612 // Type or member is obsolete
			await Device.InvokeOnMainThreadAsync(() =>
				Platform.Android.Platform.CreateRendererWithContext(visualElement,
					DependencyService.Resolve<Context>()));
#pragma warning restore CS0612 // Type or member is obsolete

			await Task.CompletedTask;
		}
	}
}