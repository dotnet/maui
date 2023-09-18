using Microsoft.Maui.Appium;
using NUnit.Framework;

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

		[Test]
		public override void _IsEnabled()
		{
			if (Device == TestDevice.Mac ||
				Device == TestDevice.iOS)
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}

			base._IsEnabled();
		}
	}
}
