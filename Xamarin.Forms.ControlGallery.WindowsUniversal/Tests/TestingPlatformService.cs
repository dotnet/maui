using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public void CreateRenderer(VisualElement visualElement)
		{
			Platform.UWP.Platform.CreateRenderer(visualElement);
		}
	}
}
