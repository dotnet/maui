using Android.Views;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	[TestFixture]
	public class RendererTests : PlatformTestFixture
	{
		[Test(Description = "Basic sanity check that Label text matches renderer text")]
		public void LabelTextMatchesRendererText()
		{
			var label = new Label { Text = "foo" };
			using (var textView = GetNativeControl(label))
			{
				Assert.That(label.Text == textView.Text);
			}
		}

		[Test(Description = "Validate renderer vertical alignment for Entry with VerticalTextAlignment Center")]
		public void EntryVerticalAlignmentCenterInRenderer()
		{ 
			var entry1 = new Entry { Text = "foo", VerticalTextAlignment = TextAlignment.Center };
			using (var editText = GetNativeControl(entry1))
			{
				var centeredVertical =
				(editText.Gravity & GravityFlags.VerticalGravityMask) == GravityFlags.CenterVertical;

				Assert.That(centeredVertical, Is.True);
			}
		}
	}
}