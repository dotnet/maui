using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	internal sealed class EventViewContainerRemote : BaseViewContainerRemote
	{
		public EventViewContainerRemote(IUIClientContext? testContext, string formsType)
			: base(testContext, formsType)
		{
		}

		public EventViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public IUIElement GetEventLabel()
		{
			App.WaitForElement(EventLabelQuery);
			return App.FindElement(EventLabelQuery);
		}
	}
}