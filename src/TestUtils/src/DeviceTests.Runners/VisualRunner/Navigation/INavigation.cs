#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public interface ITestNavigation
	{
		Task NavigateTo(PageType page, object? dataContext = null);
	}
}