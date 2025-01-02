using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public StateViewContainerRemote(IUIClientContext? testContext, string formsType)
			: base(testContext, formsType)
		{
		}

		public void TapStateButton()
		{
			App.Screenshot("Before state change");
			App.WaitForElementTillPageNavigationSettled(StateButtonQuery);
			App.Tap(StateButtonQuery);
			App.Screenshot("After state change");
		}

		public IUIElement GetStateLabel()
		{
			return App.FindElement(StateLabelQuery);
		}
	}
}