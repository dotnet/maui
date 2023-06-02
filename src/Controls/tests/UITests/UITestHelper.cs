using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	public static class UITestHelper
	{
		public static string ReadText(this AppResult result) => result.Text ?? result.Description;
	}

}