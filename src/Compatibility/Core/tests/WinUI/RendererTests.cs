using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	// [TestFixture] - removed for xUnit
	public class RendererTests : PlatformTestFixture
	{
		[Fact]
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
