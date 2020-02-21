using System.Collections;
using System.Linq;
using NUnit.Framework;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.Platform.UWP;
using WColor = Windows.UI.Color;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
{
	[TestFixture]
	public class BackgroundColorTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				// SearchBar is currently busted; when 8773 gets merged we can stop filtering it
				foreach (var element in BasicViews
					.Where(v => !(v is SearchBar))
					.Where(v => !(v is Frame)))
				{
					element.BackgroundColor = Color.AliceBlue;
					yield return CreateTestCase(element);
				}
			}
		}

		WColor GetBackgroundColor(Control control)
		{
			if (control is FormsButton button)
			{
				return (button.BackgroundColor as SolidColorBrush).Color;
			}

			if (control is StepperControl stepper)
			{
				return stepper.ButtonBackgroundColor.ToUwpColor();
			}

			return (control.Background as SolidColorBrush).Color;
		}

		WColor GetBackgroundColor(Panel panel)
		{
			return (panel.Background as SolidColorBrush).Color;
		}

		WColor GetBackgroundColor(Border border)
		{
			return (border.Background as SolidColorBrush).Color;
		}

		WColor GetNativeColor(View view)
		{
			var control = GetNativeControl(view);

			if (control != null)
			{
				return GetBackgroundColor(control);
			}

			var border = GetBorder(view);

			if (border != null)
			{
				return GetBackgroundColor(border);
			}

			var panel = GetPanel(view);
			return GetBackgroundColor(panel);
		}

		[Test, TestCaseSource(nameof(TestCases))]
		[Description("View background color should match renderer background color")]
		public void BackgroundColorConsistent(View view)
		{
			var nativeColor = GetNativeColor(view);
			var formsColor = view.BackgroundColor.ToUwpColor();
			Assert.That(nativeColor, Is.EqualTo(formsColor));
		}

		[Test, Category("BackgroundColor"), Category("Frame")]
		[Description("Frame background color should match renderer background color")]
		public void FrameBackgroundColorConsistent()
		{
			var frame = new Frame() { BackgroundColor = Color.Orange };

			var renderer = GetRenderer(frame);
			var nativeElement = renderer.GetNativeElement() as Border;

			var backgroundBrush = nativeElement.Background as SolidColorBrush;
			var actualColor = backgroundBrush.Color;

			var expectedColor = frame.BackgroundColor.ToUwpColor();
			Assert.That(actualColor, Is.EqualTo(expectedColor));
		}
	}
}
