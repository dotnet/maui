using TestUtils.Appium.UITests;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class EventViewContainerRemote : BaseViewContainerRemote
	{
		public EventViewContainerRemote(IUITestContext? testContext, string formsType)
			: base(testContext, formsType)
		{
		}

		public EventViewContainerRemote(IUITestContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public AppResult GetEventLabel()
		{
			App.WaitForElement(q => q.Raw(EventLabelQuery));
			return App.Query(q => q.Raw(EventLabelQuery)).First();
		}
	}
}