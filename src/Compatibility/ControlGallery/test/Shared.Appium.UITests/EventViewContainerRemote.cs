using UITest.Appium;
using UITest.Core;

namespace UITests
{
	internal sealed class EventViewContainerRemote : BaseViewContainerRemote
	{
		public EventViewContainerRemote(IApp app, Enum type)
			: base(app, type)
		{
		}

		public IUIElement GetEventLabel()
		{
			App.WaitForElement(EventLabelQuery);
			return App.FindElement(EventLabelQuery);
		}
	}
}