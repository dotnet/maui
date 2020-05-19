using System.Threading.Tasks;

namespace System.Maui.Controls.Tests
{
	public interface ITestingPlatformService
	{
		Task CreateRenderer(VisualElement visualElement);
	}
}
