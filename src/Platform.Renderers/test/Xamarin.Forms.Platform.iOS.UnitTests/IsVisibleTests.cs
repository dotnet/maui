using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Platform.iOS.UnitTests
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
		public async Task VisibleConsistent(View view)
		{
			var expected = !view.IsVisible;
			var actual = await GetRendererProperty(view, (ver) => ver.NativeView.Layer.Hidden, requiresLayout: true);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}