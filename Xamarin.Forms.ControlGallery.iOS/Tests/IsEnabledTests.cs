using System.Collections;
using NUnit.Framework;
using UIKit;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	public class IsEnabledTests : PlatformTestFixture 
	{
		static IEnumerable TestCases
		{
			get
			{
				// Generate IsEnabled = true cases
				foreach (var element in BasicViews)
				{
					element.IsEnabled = true;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsEnabled_{element.IsEnabled}");
				}

				// Generate IsEnabled = false cases
				foreach (var element in BasicViews)
				{
					element.IsEnabled = false;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsEnabled_{element.IsEnabled}");
				}
			}
		}

		[Test, Category("IsEnabled"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement enabled should match renderer enabled")]
		public void EnabledConsistent(View view)
		{
			using (var renderer = GetRenderer(view))
			{
				var expected = view.IsEnabled;
				var nativeView = renderer.NativeView;

				// Check the container
				Assert.That(renderer.NativeView.UserInteractionEnabled, Is.EqualTo(expected));

				// Check the actual control
				var control = GetNativeControl(view);

				if (control is UIControl uiControl)
				{
					Assert.That(uiControl.Enabled, Is.EqualTo(expected));
				}
			}
		}
	}
}