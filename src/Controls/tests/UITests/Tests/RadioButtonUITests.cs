using Microsoft.Maui.Appium;

namespace Microsoft.Maui.AppiumTests
{
	public class RadioButtonUITests : _ViewUITests
	{
		public static readonly string RadioButton = "android.widget.RadioButton";
		public const string RadioButtonGallery = "* marked:'RadioButton Core Gallery'";

		public RadioButtonUITests(TestDevice device)
			: base(device)
		{
			PlatformViewType = RadioButton;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(RadioButtonGallery);
		}
	}
}
