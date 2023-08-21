using TestUtils.Appium.UITests;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class EventViewContainerRemote : BaseViewContainerRemote
	{
		public EventViewContainerRemote(IUITestContext? testContext, Enum formsType, string? platformViewType)
			: base(testContext, formsType, platformViewType)
		{
		}

		public AppResult GetEventLabel()
		{
			App.WaitForElement(q => q.Raw(EventLabelQuery));
			return App.Query(q => q.Raw(EventLabelQuery)).First();
		}
	}
}