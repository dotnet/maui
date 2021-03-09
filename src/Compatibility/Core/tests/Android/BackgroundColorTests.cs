using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class BackgroundColorTests : PlatformTestFixture 
	{
		static IEnumerable TestCases
		{
			get
			{
				foreach (var element in BasicElements
					.Where(e => !(e is Button) && !(e is ImageButton) && !(e is Frame)))
				{
					element.BackgroundColor = Color.AliceBlue;
					yield return new TestCaseData(element)
						.SetCategory(element.GetType().Name);
				}
			}
		}

		[Test, Category("BackgroundColor"), Category("Button")]
		[Description("Button background color should match renderer background color")]
		public async Task ButtonBackgroundColorConsistent()
		{
			var button = new Button 
			{ 
				Text = "      ",
				HeightRequest = 100, WidthRequest = 100,
				BackgroundColor = Color.AliceBlue 
			};

			var expectedColor = button.BackgroundColor.ToAndroid();
			var screenshot = await GetControlProperty(button, abutton => abutton.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(expectedColor);
		}

		[Test, Category("BackgroundColor"), Category("Button")]
		[Description("ImageButton background color should match renderer background color")]
		public async Task ImageButtonBackgroundColorConsistent()
		{
			var button = new ImageButton
			{
				HeightRequest = 100,
				WidthRequest = 100,
				BackgroundColor = Color.AliceBlue
			};

			var expectedColor = button.BackgroundColor.ToAndroid();
			var screenshot = await GetControlProperty(button, abutton => abutton.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(expectedColor);
		}

		[Test, Category("BackgroundColor")]
		[Description("Frame background color should match renderer background color")]
		public async Task FrameBackgroundColorConsistent()
		{
			var frame = new Frame
			{
				HeightRequest = 100,
				WidthRequest = 100,
				BackgroundColor = Color.AliceBlue
			};

			var expectedColor = frame.BackgroundColor.ToAndroid();

			var screenshot = await GetRendererProperty(frame, ver => ver.View.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(expectedColor);
		}

		[Test, Category("BackgroundColor"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement background color should match renderer background color")]
		public async Task BackgroundColorConsistent(VisualElement element)
		{
			var expectedColor = element.BackgroundColor.ToAndroid();
			var nativeColor = await GetRendererProperty(element, ver => (ver.View.Background as ColorDrawable)?.Color);
			Assert.That(nativeColor, Is.EqualTo(expectedColor));
		}
	}
}