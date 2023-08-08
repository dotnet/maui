using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public interface IApp2 : IApp, IDisposable
	{
		void ActivateApp();
		void CloseApp();
		string ElementTree { get; }
		bool WaitForTextToBePresentInElement(string automationId, string text);
		public byte[] Screenshot();
	}
}
