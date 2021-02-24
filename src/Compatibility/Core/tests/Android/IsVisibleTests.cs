using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Microsoft.Maui.Controls.Compatibility.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class IsVisibleTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				// Generate IsVisible = true cases
				foreach (var element in BasicElements)
				{
					element.IsVisible = true;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsVisible_True");
				}

				// Generate IsVisible = false cases
				foreach (var element in BasicElements)
				{
					element.IsVisible = false;
					yield return CreateTestCase(element)
						.SetName($"{element.GetType().Name}_IsVisible_False");
				}
			}
		}

		[Test, Category("IsVisible"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement visibility should match renderer visibility")]
		public async Task VisibleConsistent(VisualElement element)
		{
			var expected = element.IsVisible
					? global::Android.Views.ViewStates.Visible
					: global::Android.Views.ViewStates.Invisible;

			var actual = await GetRendererProperty(element, ver => ver.View.Visibility, requiresParent: true);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}