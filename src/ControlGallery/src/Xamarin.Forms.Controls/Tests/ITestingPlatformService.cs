using System.Threading.Tasks;

namespace Xamarin.Forms.Controls.Tests
{
	public interface ITestingPlatformService
	{
		Task CreateRenderer(VisualElement visualElement);
	}
}
