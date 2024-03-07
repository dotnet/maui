using UITest.Appium;
using UITest.Core;

namespace UITests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote(IApp app, Enum type)
			: base(app, type)
		{
		}

		public void TapStateButton()
		{
			App.Tap(StateButtonQuery);
		}

		public IUIElement GetStateLabel()
		{
			return App.FindElement(StateLabelQuery);
		}
	}
}