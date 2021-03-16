using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests
{
	public interface ITestingPlatformService
	{
		Task CreateRenderer(VisualElement visualElement);
	}
}
