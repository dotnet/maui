using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using WColor = Windows.UI.Color;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
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
				return (button.BackgroundColor as WSolidColorBrush).Color;
			}

			if (control is StepperControl stepper)
			{
				return stepper.ButtonBackgroundColor.ToWindowsColor();
			}

			return (control.Background as WSolidColorBrush).Color;
		}

		WColor GetBackgroundColor(Panel panel)
		{
			return (panel.Background as WSolidColorBrush).Color;
		}

		WColor GetBackgroundColor(Border border)
		{
			return (border.Background as WSolidColorBrush).Color;
		}

		async Task<WColor> GetNativeColor(View view)
		{
			return await Device.InvokeOnMainThreadAsync(() =>
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
			});
		}

		[Test, TestCaseSource(nameof(TestCases))]
		[Description("View background color should match renderer background color")]
		public async Task BackgroundColorConsistent(View view)
		{
			var nativeColor = await GetNativeColor(view);
			var formsColor = view.BackgroundColor.ToWindowsColor();
			Assert.That(nativeColor, Is.EqualTo(formsColor));
		}

		[Test, Category("BackgroundColor"), Category("Frame")]
		[Description("Frame background color should match renderer background color")]
		public async Task FrameBackgroundColorConsistent()
		{
			var frame = new Frame() { BackgroundColor = Color.Orange };
			var expectedColor = frame.BackgroundColor.ToWindowsColor();

			var actualColor = await Device.InvokeOnMainThreadAsync(() =>
			{
				var renderer = GetRenderer(frame);
				var nativeElement = renderer.GetNativeElement() as Border;

				var backgroundBrush = nativeElement.Background as WSolidColorBrush;
				return backgroundBrush.Color;
			});

			Assert.That(actualColor, Is.EqualTo(expectedColor));
		}
	}
}
