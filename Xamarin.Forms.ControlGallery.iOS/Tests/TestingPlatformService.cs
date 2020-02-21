using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public void CreateRenderer(VisualElement visualElement)
		{
			Platform.iOS.Platform.CreateRenderer(visualElement);
		}
	}
}