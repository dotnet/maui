using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23897(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "Add a permanent wrapper around ImageButton so it works better with loading and unloading";

		[Fact]
		[Category(UITestCategories.ImageButton)]
		public void LoadingAndUnloadingWorksForImageButton()
		{
			for (int i = 0; i < 3; i++)
			{
				var loadedCount = App.WaitForElement("LoadedCount").GetText();
				var unLoadedCount = App.WaitForElement("UnloadedCount").GetText();
				Assert.Equal($"{i + 1}", loadedCount);
				Assert.Equal($"{i}", unLoadedCount);

				App.Tap("PushAndPopPage");
			}
		}
	}
}