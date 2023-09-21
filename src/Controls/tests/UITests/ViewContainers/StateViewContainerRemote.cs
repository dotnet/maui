using TestUtils.Appium.UITests;
using Xamarin.UITest.Queries;

namespace Microsoft.Maui.AppiumTests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IUITestContext? testContext, Enum formsType)
			: base(testContext, formsType)
		{
		}

		public void TapStateButton()
		{
			App.Screenshot("Before state change");
			App.Tap(q => q.Raw(StateButtonQuery));
			App.Screenshot("After state change");
		}

		public AppResult GetStateLabel()
		{
			return App.Query(q => q.Raw(StateLabelQuery)).First();
		}
	}
}