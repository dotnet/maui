using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public interface IUITestContext : IDisposable
	{
		public IApp App { get; }
		public TestConfig TestConfig { get; }
	}
}
