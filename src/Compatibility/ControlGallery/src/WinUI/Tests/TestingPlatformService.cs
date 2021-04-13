using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(TestingPlatformService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows.Tests
{
	class TestingPlatformService : ITestingPlatformService
	{
		public async Task CreateRenderer(VisualElement visualElement)
		{
			await Device.InvokeOnMainThreadAsync(() => Platform.UWP.Platform.CreateRenderer(visualElement));
		}
	}
}
