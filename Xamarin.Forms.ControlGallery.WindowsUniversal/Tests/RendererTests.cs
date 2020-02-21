using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
{
	[TestFixture]
	public class RendererTests : PlatformTestFixture
	{
		[Test]
		[Description("Basic sanity check that Label text matches renderer text")]
		public void LabelTextMatchesRendererText()
		{
			var label = new Label { Text = "foo" };
			var textBlock = GetNativeControl(label);
			Assert.That(label.Text == textBlock.Text);
		}
	}
}
