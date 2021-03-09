using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class BackgroundColorTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				foreach (var element in BasicViews
					.Where(e => !(e is Label) && !(e is BoxView) && !(e is Frame)))
				{
					element.BackgroundColor = Color.AliceBlue;
					yield return new TestCaseData(element)
						.SetCategory(element.GetType().Name);
				}
			}
		}

		[Test, Category("BackgroundColor"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement background color should match renderer background color")]
		public async Task BackgroundColorConsistent(VisualElement element)
		{
			var expected = element.BackgroundColor.ToUIColor();
			var actual = await GetControlProperty(element, uiview => uiview.BackgroundColor);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("BackgroundColor"), Category("Frame")]
		[Description("Frame background color should match renderer background color")]
		public async Task FrameBackgroundColorConsistent()
		{
			var frame = new Frame { BackgroundColor = Color.AliceBlue };
			var expectedColor = frame.BackgroundColor.ToUIColor();
			var screenshot = await GetRendererProperty(frame, (ver) => ver.NativeView.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(expectedColor);
		}

		[Test, Category("BackgroundColor"), Category("Label")]
		[Description("Label background color should match renderer background color")]
		public async Task LabelBackgroundColorConsistent()
		{
			var label = new Label { Text = "foo", BackgroundColor = Color.AliceBlue };
			var expected = label.BackgroundColor.ToUIColor();
			var actual = await GetRendererProperty(label, r => r.NativeView.BackgroundColor);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("BackgroundColor"), Category("BoxView")]
		[Description("BoxView background color should match renderer background color")]
		public async Task BoxViewBackgroundColorConsistent2()
		{
			var boxView = new BoxView { BackgroundColor = Color.AliceBlue };
			var expectedColor = boxView.BackgroundColor.ToUIColor();
			var screenshot = await GetRendererProperty(boxView, (ver) => ver.NativeView.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(expectedColor);
		}
	}
}