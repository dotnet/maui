using Microsoft.Maui.Appium;

namespace Microsoft.Maui.AppiumTests
{
	public class CheckBoxUITests : _ViewUITests
	{
		static readonly string CheckBox = "android.widget.CheckBox";
		const string CheckBoxGallery = "* marked:'CheckBox Gallery'";

		public CheckBoxUITests(TestDevice device)
			: base(device)
		{
			PlatformViewType = CheckBox;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(CheckBoxGallery);
		}
	}
}
