using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class OpacityTests : PlatformTestFixture
	{
		static readonly double TestOpacity = 0.4;

		static IEnumerable TestCases
		{
			get
			{
				foreach (var element in BasicViews.Where(e => !(e is Label)))
				{
					element.Opacity = TestOpacity;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Opacity"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement opacity should match renderer opacity")]
		public void OpacityConsistent(View view)
		{
			var page = new ContentPage() { Content = view };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(view).NativeView)
				{
					page.Layout(new Rectangle(0, 0, 200, 200));

					var expected = view.Opacity;

					// Deliberately casting this to double because Within doesn't seem to grasp nfloat
					// If you write this the other way around (casting expected to an nfloat), it fails
					Assert.That((double)uiView.Alpha, Is.EqualTo(expected).Within(0.001d));
				}
			}
		}

		[Test, Category("Opacity"), Category("Label")]
		[Description("Label opacity should match renderer opacity")]
		public void LabelOpacityConsistent()
		{
			var label = new Label { Text = "foo", Opacity = TestOpacity };

			var page = new ContentPage() { Content = label };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var renderer = GetRenderer(label))
				{
					page.Layout(new Rectangle(0, 0, 200, 200));
					var expected = label.Opacity;
					Assert.That((double)renderer.NativeView.Alpha, Is.EqualTo(expected).Within(0.001d));
				}
			}
		}
	}
}