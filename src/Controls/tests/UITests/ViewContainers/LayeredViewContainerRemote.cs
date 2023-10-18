using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class LayeredViewContainerRemote : BaseViewContainerRemote
	{
		public LayeredViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public IUIElement GetLayeredLabel()
		{
			return App.FindElement(LayeredLabelQuery);
		}

		public void TapHiddenButton()
		{
			App.Click(LayeredHiddenButtonQuery);
		}
	}
}