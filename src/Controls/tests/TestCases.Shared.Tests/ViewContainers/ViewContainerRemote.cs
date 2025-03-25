using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
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