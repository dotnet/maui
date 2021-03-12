using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery.Tests
{
	public interface ITestingPlatformService
	{
		Task CreateRenderer(VisualElement visualElement);
	}
}
