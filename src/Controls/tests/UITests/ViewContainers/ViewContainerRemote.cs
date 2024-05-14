using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class ViewContainerRemote : BaseViewContainerRemote
	{
		public ViewContainerRemote(IUIClientContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public ViewContainerRemote(IUIClientContext? testContext, string formsType)
			: base(testContext, formsType)
		{
		}
	}
}