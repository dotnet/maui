using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	internal sealed class LayeredViewContainerRemote : BaseViewContainerRemote
	{
		public LayeredViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public LayeredViewContainerRemote(IUIClientContext? testContext, string formsType)
			: base(testContext, formsType)
		{
		}

		public IUIElement GetLayeredLabel()
		{
			return App.FindElement(LayeredLabelQuery);
		}

		public void TapHiddenButton()
		{
			App.Tap(LayeredHiddenButtonQuery);
		}
	}
}