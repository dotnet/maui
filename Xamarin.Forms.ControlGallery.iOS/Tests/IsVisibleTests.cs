using System.Collections;
using NUnit.Framework;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	public class IsVisibleTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				// Generate IsVisible = true cases
				foreach (var element in BasicViews)
				{
					element.IsVisible = true;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsVisible_{element.IsVisible}");
				}

				// Generate IsEnabled = false cases
				foreach (var element in BasicViews)
				{
					element.IsVisible = false;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsVisible_{element.IsVisible}");
				}
			}
		}

		[Test, Category("IsVisible"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement visibility should match renderer visibility")]
		public void VisibleConsistent(View view)
		{
			var page = new ContentPage() { Content = view };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(view).NativeView)
				{
					page.Layout(new Rectangle(0, 0, 200, 200));

					var expectedHidden = !view.IsVisible;
					var actualHidden = uiView.Layer.Hidden;

					Assert.That(actualHidden, Is.EqualTo(expectedHidden));
				}
			}
		}
	}
}