using System.Collections;
using System.Linq;
using NUnit.Framework;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
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
		public void BackgroundColorConsistent(VisualElement element)
		{
			using (var uiView = GetNativeControl(element))
			{
				var expectedColor = element.BackgroundColor.ToUIColor();
				Assert.That(uiView.BackgroundColor, Is.EqualTo(expectedColor));
			}
		}

		[Test, Category("BackgroundColor"), Category("Frame")]
		[Description("Frame background color should match renderer background color")]
		public void FrameBackgroundColorConsistent()
		{
			var frame = new Frame { BackgroundColor = Color.AliceBlue };
			using (var renderer = GetRenderer(frame))
			{
				var expectedColor = frame.BackgroundColor.ToUIColor();
				Assert.That(renderer.NativeView.BackgroundColor, Is.EqualTo(expectedColor));
			}
		}

		[Test, Category("BackgroundColor"), Category("Label")]
		[Description("Label background color should match renderer background color")]
		public void LabelBackgroundColorConsistent()
		{
			var label = new Label { Text = "foo", BackgroundColor = Color.AliceBlue };
			using (var renderer = GetRenderer(label))
			{
				var expectedColor = label.BackgroundColor.ToUIColor();
				Assert.That(renderer.NativeView.BackgroundColor, Is.EqualTo(expectedColor));
			}
		}

		[Test, Category("BackgroundColor"), Category("BoxView")]
		[Description("BoxView background color should match renderer background color")]
		public void BoxViewBackgroundColorConsistent()
		{
			var boxView = new BoxView { BackgroundColor = Color.AliceBlue };
			var expectedColor = boxView.BackgroundColor.ToUIColor();

			var page = new ContentPage() { Content = boxView };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(boxView).NativeView)
				{
					page.Layout(new Rectangle(0, 0, 200, 200));

					uiView.AssertColorAtCenter(expectedColor);
				}
			}
		}
	}
}