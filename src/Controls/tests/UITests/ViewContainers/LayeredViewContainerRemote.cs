using TestUtils.Appium.UITests;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class LayeredViewContainerRemote : BaseViewContainerRemote
	{
		public LayeredViewContainerRemote(IUITestContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public AppResult GetLayeredLabel()
		{
			return App.Query(q => q.Raw(LayeredLabelQuery)).First();
		}

		public void TapHiddenButton()
		{
			App.Tap(q => q.Raw(LayeredHiddenButtonQuery));
		}
	}
}