using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public void TapStateButton()
		{
			App.Screenshot("Before state change");
			App.Tap(StateButtonQuery);
			App.Screenshot("After state change");
		}

		public IUIElement GetStateLabel()
		{
			return App.FindElement(StateLabelQuery);
		}
	}
}