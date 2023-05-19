using Microsoft.Maui.Appium;

namespace Microsoft.Maui.AppiumTests
{
	public class EditorUITests : _ViewUITests
	{
		public static readonly string Editor = "android.widget.EditorEditText";
		public const string EditorGallery = "* marked:'Editor Gallery'";

		public EditorUITests(TestDevice device) 
			: base(device)
		{
			PlatformViewType = Editor;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(EditorGallery);
		}
	}
}
