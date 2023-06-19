using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public interface IApp2 : IApp, IDisposable
	{
		void ActivateApp();
		void CloseApp();
		string ElementTree { get; }
	}
}
