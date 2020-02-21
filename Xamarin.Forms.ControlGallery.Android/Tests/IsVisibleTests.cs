using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.ControlGallery.Android.Tests
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
		public void VisibleConsistent(VisualElement element)
		{
			using (var renderer = GetRenderer(element))
			{
				var expected = element.IsVisible 
				? global::Android.Views.ViewStates.Visible 
				: global::Android.Views.ViewStates.Invisible;
				
				var nativeView = renderer.View;

				ParentView(nativeView);

				Assert.That(renderer.View.Visibility, Is.EqualTo(expected));

				UnparentView(nativeView);
			}
		}
	}
}