using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.UnitTests
{
	public class TestBase
	{
		static TestBase()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}
	}
}
