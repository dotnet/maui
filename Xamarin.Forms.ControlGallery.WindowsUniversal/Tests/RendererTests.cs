using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
{
	[TestFixture]
	public class RendererTests : PlatformTestFixture
	{
		[Test]
		[Description("Basic sanity check that Label text matches renderer text")]
		public async Task LabelTextMatchesRendererText()
		{
			var label = new Label { Text = "foo" };
			var expected = label.Text;
			var actual = await GetControlProperty(label, tb => tb.Text);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}
